using UnityEngine;

public enum PetState { Idle, Move, Interact, Eat }

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
        // 상호작용 중이 아닐 때만 자율 AI 작동
        if (currentState == PetState.Idle || currentState == PetState.Move)
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
        if (currentState == PetState.Idle)
        {
            ChangeState(PetState.Move);
            SetRandomNextChangeTime(3f, 6f); // 이동 지속 시간
        }
        else if (currentState == PetState.Move)
        {
            ChangeState(PetState.Idle);
            SetRandomNextChangeTime(2f, 5f); // 대기 지속 시간
        }
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
        }

        Debug.Log($"[FSM] 상태 변경: {newState}");
    }

    public PetState GetCurrentState() => currentState;
}
