using UnityEngine;

public enum PetState { Idle, Move, Interact, Eat, Struggling }

public class PetStateMachine : MonoBehaviour
{
    [SerializeField] private PetState currentState;
    private PetMovement _movement;
    private PetVisualManager _visualManager;
    private PetGrowthController _growthController;
    private float _stateTimer;
    private float _nextChangeTime;

    void Awake()
    {
        _movement = GetComponent<PetMovement>();
        _visualManager = GetComponent<PetVisualManager>();
        _growthController = GetComponent<PetGrowthController>();
    }

    void Start()
    {
        // 시작 시 랜덤한 지연 시간 설정 후 이동 상태로 시작
        SetRandomNextChangeTime(1f, 3f);
        ChangeState(PetState.Move);
    }

    void Update()
    {
        // Struggling(붙잡힘) 상태일 때는 AI 타이머 및 상태 전환을 완전히 중단합니다.
        // Eat(밥먹기), Interact(특수동작) 중일 때는 타이머는 흐르지만, 
        // 애니메이션이 끝날 때까지 대기하도록 AutoToggleState 내부에서 처리될 수 있습니다.
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
        // AI에 의한 강제 상태 전환 로직
        
        // 1. 특수 동작 확률 체크
        if (Random.value < 0.2f)
        {
            if (TryPlayRandomSpecialAction())
            {
                SetRandomNextChangeTime(2f, 4f); 
                return;
            }
        }

        // 2. 현재 상태에 따른 다음 상태 결정
        // (Struggling 상태는 여기서 제외하여 AI가 멋대로 이동 상태로 바꾸지 못하게 함)
        if (currentState == PetState.Idle || currentState == PetState.Interact)
        {
            ChangeState(PetState.Move);
            SetRandomNextChangeTime(3f, 6f);
        }
        else if (currentState == PetState.Move)
        {
            ChangeState(PetState.Idle);
            SetRandomNextChangeTime(2f, 5f);
        }
        // Struggling 상태일 때는 AI가 개입하지 않고 마우스를 뗄 때까지 대기함
    }

    private bool TryPlayRandomSpecialAction()
    {
        if (_visualManager == null || _visualManager.petDefinition == null || _growthController == null) return false;

        var info = _visualManager.petDefinition.GetStageInfo(_growthController.currentData.currentStage);
        if (info == null || info.specialActions == null || info.specialActions.Count == 0) return false;

        // 랜덤하게 하나 선택
        int index = Random.Range(0, info.specialActions.Count);
        string actionName = info.specialActions[index].actionName;

        ChangeState(PetState.Interact); // Interact 상태를 빌려씀 (정지 상태)
        _visualManager.PlayAction(actionName);
        Debug.Log($"[AI] 특수 동작 재생: {actionName}");
        return true;
    }

    private void SetRandomNextChangeTime(float min, float max)
    {
        _nextChangeTime = UnityEngine.Random.Range(min, max);
    }

    public void ChangeState(PetState newState, bool resetTarget = true)
    {
        if (_visualManager == null) return;

        currentState = newState;
        _stateTimer = 0f; // 상태 변경 시 타이머 초기화

        switch (newState)
        {
            case PetState.Idle:
                _visualManager.SetAnimationState("isMoving", false);
                if (_movement != null) _movement.isLocked = true;
                break;

            case PetState.Move:
                _visualManager.SetAnimationState("isMoving", true);
                if (_movement != null)
                {
                    _movement.isLocked = false;
                    if (resetTarget)
                    {
                        _movement.SetNewRandomTarget(); // 새 목표 설정 (기본값)
                    }
                }
                break;

            case PetState.Interact:
                _visualManager.TriggerAnimation("interact");
                if (_movement != null) _movement.isLocked = true;
                break;

            case PetState.Eat:
                _visualManager.TriggerAnimation("eat");
                if (_movement != null) _movement.isLocked = true;
                break;

            case PetState.Struggling:
                if (_visualManager != null && _visualManager.petDefinition != null && _growthController != null)
                {
                    PetStageInfo info = _visualManager.petDefinition.GetStageInfo(_growthController.currentData.currentStage);
                    if (info != null && info.is2D && info.struggleSpriteSheet != null)
                    {
                        _visualManager.UpdateSpriteSheet(info.struggleSpriteSheet, info.struggleFrameCount);
                    }
                    else if (info != null && !info.is2D)
                    {
                        _visualManager.TriggerAnimation("struggle");
                    }
                }
                if (_movement != null) _movement.isLocked = true;
                break;
        }

        Debug.Log($"[FSM] {newState} (ResetTarget: {resetTarget})");
    }

    public PetState GetCurrentState() => currentState;
}
