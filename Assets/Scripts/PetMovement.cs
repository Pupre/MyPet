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
    private float _timer;

    void Start()
    {
        SetNewRandomTarget();
    }

    void Update()
    {
        if (isLocked) return;

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

        // Lerp 대신 MoveTowards를 사용하여 일정한 속도로 이동 ("슝 슝" 날아가는 느낌 제거)
        Vector3 newPos = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
        
        // 이동 방향에 따라 펫 좌우 반전 (스케일을 뒤집는 방식)
        if (newPos.x != transform.position.x)
        {
            Vector3 localScale = transform.localScale;
            bool isMovingRight = newPos.x > transform.position.x;
            // 2D 스프라이트의 경우 Flip을 처리하는 방식이 다를 수 있으나, 일반적으로 부모 스케일 조정을 많이 사용함
            // 여기서는 단순함을 위해 방향 변수만 계산함 (VisualManager에서 처리하도록 보강 가능)
        }

        transform.position = newPos;

        // 목표 지점에 거의 도달했는지 확인
        if (useRandomMovement && Vector3.Distance(transform.position, _randomTarget) < 0.1f)
        {
            // 목표 도달 알림을 StateMachine 등에 보낼 수 있음
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
