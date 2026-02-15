using UnityEngine;

public enum PetState { Idle, Move, Interact, Eat, Struggling }

public class PetStateMachine : MonoBehaviour
{
    public static PetStateMachine Instance { get; private set; }

    [SerializeField] private PetState currentState;
    private PetMovement _movement;
    private float _stateTimer;
    private float _nextChangeTime;

    void Awake()
    {
        if (Instance == null) Instance = this;
        _movement = GetComponent<PetMovement>();
    }

    void Start()
    {
        // 시작 시 랜덤한 지연 시간 설정 후 이동 상태로 시작
        SetRandomNextChangeTime(1f, 3f);
        ChangeState(PetState.Move);
    }

    void Update()
    {
        // 상호작용(Interact, Eat) 중이거나 대기/이동 중일 때 자율 AI 타이머 작동
        // (Struggling 상태일 때는 타이머가 멈춰서 계속 아둥바둥함)
        if (currentState != PetState.Struggling)
        {
            _stateTimer += Time.deltaTime;
            if (_stateTimer >= _nextChangeTime)
            {
                _stateTimer = 0f;
                AutoToggleState();
            }
        }
    }

    private void AutoToggleState()
    {
        // 20% 확률로 특수 동작(Special Action) 수행
        if (Random.value < 0.2f)
        {
            if (TryPlayRandomSpecialAction())
            {
                SetRandomNextChangeTime(2f, 4f); // 특수 동작 지속 시간
                return;
            }
        }

        if (currentState == PetState.Idle || currentState == PetState.Interact || currentState == PetState.Struggling)
        {
            ChangeState(PetState.Move);
            SetRandomNextChangeTime(3f, 6f); // 이동 지속 시간
        }
        else
        {
            ChangeState(PetState.Idle);
            SetRandomNextChangeTime(2f, 5f); // 대기 지속 시간
        }
    }

    private bool TryPlayRandomSpecialAction()
    {
        if (PetVisualManager.Instance == null || PetVisualManager.Instance.petDefinition == null) return false;

        var info = PetVisualManager.Instance.petDefinition.GetStageInfo(PetGrowthController.Instance.currentData.currentStage);
        if (info == null || info.specialActions == null || info.specialActions.Count == 0) return false;

        // 랜덤하게 하나 선택
        int index = Random.Range(0, info.specialActions.Count);
        string actionName = info.specialActions[index].actionName;

        ChangeState(PetState.Interact); // Interact 상태를 빌려씀 (정지 상태)
        PetVisualManager.Instance.PlayAction(actionName);
        Debug.Log($"[AI] 특수 동작 재생: {actionName}");
        return true;
    }

    private void SetRandomNextChangeTime(float min, float max)
    {
        _nextChangeTime = UnityEngine.Random.Range(min, max);
    }

    public void ChangeState(PetState newState)
    {
        if (PetVisualManager.Instance == null) return;

        currentState = newState;
        _stateTimer = 0f; // 상태 변경 시 타이머 초기화

        switch (newState)
        {
            case PetState.Idle:
                PetVisualManager.Instance.SetAnimationState("isMoving", false);
                if (_movement != null) _movement.isLocked = true;
                break;

            case PetState.Move:
                PetVisualManager.Instance.SetAnimationState("isMoving", true);
                if (_movement != null)
                {
                    _movement.isLocked = false;
                    _movement.SetNewRandomTarget(); // 새 목표 설정
                }
                break;

            case PetState.Interact:
                PetVisualManager.Instance.TriggerAnimation("interact");
                if (_movement != null) _movement.isLocked = true;
                break;

            case PetState.Eat:
                PetVisualManager.Instance.TriggerAnimation("eat");
                if (_movement != null) _movement.isLocked = true;
                break;

            case PetState.Struggling:
                if (PetVisualManager.Instance.petDefinition != null)
                {
                    PetStageInfo info = PetVisualManager.Instance.petDefinition.GetStageInfo(PetGrowthController.Instance.currentData.currentStage);
                    if (info != null && info.is2D && info.struggleSpriteSheet != null)
                    {
                        PetVisualManager.Instance.UpdateSpriteSheet(info.struggleSpriteSheet, info.struggleFrameCount);
                    }
                    else if (info != null && !info.is2D)
                    {
                        PetVisualManager.Instance.TriggerAnimation("struggle");
                    }
                }
                if (_movement != null) _movement.isLocked = true;
                break;
        }

        Debug.Log($"[FSM] 상태 변경: {newState}");
    }

    public PetState GetCurrentState() => currentState;
}
