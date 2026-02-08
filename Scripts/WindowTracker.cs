using UnityEngine;
using System;

public class WindowTracker : MonoBehaviour
{
    public string activeWindowTitle; // 실제 제목을 가져오려면 추가 API 필요 (현재는 위치 위주)
    public Vector2 targetWindowPos;
    public Vector2 targetWindowSize;

    void Update()
    {
        IntPtr activeHWnd;
        Win32Bridge.RECT rect = Win32Bridge.Instance.GetActiveWindowRect(out activeHWnd);

        // 현재 유니티 창 자신은 제외 (유니티 창이 활성화되면 추적 중단하거나 무시)
        // 실제 구현 시에는 내 창의 Handle과 비교하는 로직 필요

        targetWindowPos = new Vector2(rect.Left, rect.Top);
        targetWindowSize = new Vector2(rect.Right - rect.Left, rect.Bottom - rect.Top);

        // 테스트를 위해 로그 출력 (너무 자주 찍히지 않게 조절 가능)
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Active Window Pos: {targetWindowPos}, Size: {targetWindowSize}");
        }
    }
}
