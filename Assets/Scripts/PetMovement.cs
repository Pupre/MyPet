using UnityEngine;

public class PetMovement : MonoBehaviour
{
    public WindowTracker tracker;
    public float moveSpeed = 2.0f;
    public Vector3 idleOffset = Vector3.zero;

    [Header("Random Movement")]
    public bool useRandomMovement = true;
    public bool isLocked = false; // 상호작용 중 이동 정지용
    public float changeTargetInterval = 3.0f;
    private Vector3 _randomTarget;

    void Start()
    {
        SetNewRandomTarget();
    }

    void Update()
    {
        if (isLocked) return;

        // 허기(Hunger)에 따른 속도 감소 로직
        float hungerMultiplier = 1.0f;
        if (PetNeedsController.Instance != null)
        {
            float hunger = PetNeedsController.Instance.GetHungerNormalized();
            // 허기가 20% 미만이면 속도를 50%로 줄임
            if (hunger < 0.2f) hungerMultiplier = 0.5f;
        }

        Vector3 destination;

        if (useRandomMovement)
        {
            destination = _randomTarget;
        }
        else if (tracker != null && tracker.hasActiveWindow)
        {
            destination = tracker.targetWorldPos + idleOffset;
        }
        else
        {
            destination = Vector3.zero;
        }

        // Lerp 대신 MoveTowards를 사용하여 일정한 속도로 이동 (허기 페널티 적용)
        Vector3 newPos = Vector3.MoveTowards(transform.position, destination, moveSpeed * hungerMultiplier * Time.deltaTime);

        transform.position = newPos;

        // 목표 지점에 거의 도달했는지 확인
        if (useRandomMovement && Vector3.Distance(transform.position, _randomTarget) < 0.1f)
        {
            OnTargetReached();
        }
    }

    private void OnTargetReached()
    {
        // 목표에 도달하면 상태 머신에게 알려서 다음 행동을 결정하게 함
        if (PetStateMachine.Instance != null && PetStateMachine.Instance.GetCurrentState() == PetState.Move)
        {
            PetStateMachine.Instance.ChangeState(PetState.Idle);
        }
    }

    public void SetNewRandomTarget()
    {
        float rangeX = 7.0f;
        float rangeY = 4.0f;
        _randomTarget = new Vector3(UnityEngine.Random.Range(-rangeX, rangeX), UnityEngine.Random.Range(-rangeY, rangeY), transform.position.z);
    }
}
