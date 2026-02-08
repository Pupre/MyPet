using UnityEngine;
using System;

public class WindowTracker : MonoBehaviour
{
    public string activeWindowTitle; // 실제 제목을 가져오려면 추가 API 필요 (현재는 위치 위주)
    public Vector2 targetWindowPos;
    public Vector2 targetWindowSize;

    public Vector3 targetWorldPos;
    public bool hasActiveWindow;

    private Camera _mainCam;

    void Start()
    {
        _mainCam = Camera.main;
    }

    void Update()
    {
        IntPtr activeHWnd;
        Win32Bridge.RECT rect = Win32Bridge.Instance.GetActiveWindowRect(out activeHWnd);

        // 창이 없거나 최소화 된 경우 등 예외 처리 
        if (rect.Right - rect.Left <= 0 || rect.Bottom - rect.Top <= 0)
        {
            hasActiveWindow = false;
            return;
        }

        hasActiveWindow = true;

        targetWindowPos = new Vector2(rect.Left, rect.Top);
        targetWindowSize = new Vector2(rect.Right - rect.Left, rect.Bottom - rect.Top);

        // Win32 좌표 (Top-Left 기준) -> Unity Screen 좌표 (Bottom-Left 기준)
        // X는 동일, Y는 Screen.height - Win32_Y
        float screenX = rect.Left + (targetWindowSize.x / 2.0f); // 창의 가로 중앙
        float screenY = Screen.height - rect.Top; // 창의 상단 (유니티 기준)

        // 화면 밖으로 너무 나갔는지 체크 (선택 사항)
        
        // Screen -> World 변환 (Z는 카메라 앞 10 유닛)
        Vector3 screenPos = new Vector3(screenX, screenY, 10.0f);
        targetWorldPos = _mainCam.ScreenToWorldPoint(screenPos);
        targetWorldPos.z = 0; // 2D 평면 이동을 위해 Z 고정

        // 테스트 로그 (디버깅용)
        // if (Time.frameCount % 120 == 0) Debug.Log($"Target: {targetWorldPos}");
    }
}
