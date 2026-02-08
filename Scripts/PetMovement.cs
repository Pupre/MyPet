using UnityEngine;

public class PetMovement : MonoBehaviour
{
    public WindowTracker tracker;
    public float moveSpeed = 5.0f;
    public Vector3 idleOffset = new Vector3(0, 0, 0); // 창 테두리에서 얼마나 띄울지

    void Update()
    {
        if (tracker == null) return;

        Vector3 destination;

        if (tracker.hasActiveWindow)
        {
            // 활성 창의 상단 중앙 + 오프셋
            destination = tracker.targetWorldPos + idleOffset;
        }
        else
        {
            // 활성 창이 없으면 화면 중앙에서 대기 (혹은 바닥으로 이동)
            destination = Vector3.zero; 
        }

        // 부드러운 이동 (Lerp)
        transform.position = Vector3.Lerp(transform.position, destination, moveSpeed * Time.deltaTime);

        // 바라보는 방향? (나중에 추가: Scale X를 뒤집거나 회전)
    }
}
