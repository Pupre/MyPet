using UnityEngine;

public class PetMovement : MonoBehaviour
{
    public WindowTracker tracker;
    public float moveSpeed = 2.0f;
    public Vector3 idleOffset = Vector3.zero;

    [Header("Random Movement")]
    public bool useRandomMovement = true;
    public float changeTargetInterval = 3.0f;
    private Vector3 _randomTarget;
    private float _timer;

    void Start()
    {
        SetNewRandomTarget();
    }

    void Update()
    {
        Vector3 destination;

        if (useRandomMovement)
        {
            _timer += Time.deltaTime;
            if (_timer >= changeTargetInterval)
            {
                SetNewRandomTarget();
                _timer = 0;
            }
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

        transform.position = Vector3.Lerp(transform.position, destination, moveSpeed * Time.deltaTime);
    }

    private void SetNewRandomTarget()
    {
        // 화면 안에서 적당한 랜덤 좌표 생성 (Orthographic 카메라 기준 대략적인 범위)
        float rangeX = 8.0f;
        float rangeY = 4.5f;
        _randomTarget = new Vector3(UnityEngine.Random.Range(-rangeX, rangeX), UnityEngine.Random.Range(-rangeY, rangeY), 0);
    }
}
