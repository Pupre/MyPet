using UnityEngine;

public enum PetState { Idle, Move, Interact, Eat }

public class PetStateMachine : MonoBehaviour
{
    public static PetStateMachine Instance { get; private set; }

    [SerializeField] private PetState currentState;
    private PetMovement _movement;

    void Awake()
    {
        if (Instance == null) Instance = this;
        _movement = GetComponent<PetMovement>();
    }

    void Start()
    {
        ChangeState(PetState.Idle);
    }

    public void ChangeState(PetState newState)
    {
        currentState = newState;

        // Visual Manager에 애니메이션 파라미터 전달
        switch (newState)
        {
            case PetState.Idle:
                PetVisualManager.Instance.SetAnimationState("isMoving", false);
                if (_movement != null) _movement.isLocked = true;
                break;

            case PetState.Move:
                PetVisualManager.Instance.SetAnimationState("isMoving", true);
                if (_movement != null) _movement.isLocked = false;
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
