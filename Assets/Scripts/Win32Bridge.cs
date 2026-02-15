using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class Win32Bridge : MonoBehaviour
{
    public static Win32Bridge Instance { get; private set; }

    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

    [DllImport("user32.dll")]
    public static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("dwmapi.dll")]
    public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    [DllImport("user32.dll")]
    public static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    public const int GWL_EXSTYLE = -20;
    public const int GWL_STYLE = -16;
    public const uint WS_EX_LAYERED = 0x00080000;
    public const uint WS_EX_TRANSPARENT = 0x00000020;
    public const uint LWA_COLORKEY = 0x00000001;
    public const uint LWA_ALPHA = 0x00000002;
    public const uint WS_POPUP = 0x80000000;
    public const uint WS_VISIBLE = 0x10000000;

    public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_SHOWWINDOW = 0x0040;

    private IntPtr _hWnd;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        _hWnd = GetActiveWindow();
        #endif
    }

    public void SetTransparency(bool enabled)
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        if (enabled)
        {
            // 스타일을 POPUP으로 변경하여 테두리 제거 (선택 사항이나 투명화에 도움됨)
            // uint style = GetWindowLong(_hWnd, -16); // GWL_STYLE
            // SetWindowLong(_hWnd, -16, WS_POPUP | WS_VISIBLE);

            // Layered 스타일 강제 적용
            uint exStyle = GetWindowLong(_hWnd, GWL_EXSTYLE);
            SetWindowLong(_hWnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);

            // DWM 확장 (Windows 10/11 가시 투명화의 핵심)
            MARGINS margins = new MARGINS { cxLeftWidth = -1 };
            DwmExtendFrameIntoClientArea(_hWnd, ref margins);

            // 추가적으로 Alpha 레이어 속성 설정 (필요한 경우)
            // SetLayeredWindowAttributes(_hWnd, 0, 255, LWA_ALPHA);
        }
        #endif
    }

    public void SetColorKey(Color32 color)
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        // 0x00BBGGRR 형식의 COLORREF 생성
        uint colorRef = (uint)((color.b << 16) | (color.g << 8) | color.r);
        
        // 레이어드 윈도우 속성 설정 (LWA_COLORKEY)
        // 이 함수는 해당 색상을 완전히 투명하게 만듭니다.
        SetLayeredWindowAttributes(_hWnd, colorRef, 0, LWA_COLORKEY);
        #endif
    }

    public void SetClickThrough(bool enabled)
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        uint style = GetWindowLong(_hWnd, GWL_EXSTYLE);
        if (enabled)
            SetWindowLong(_hWnd, GWL_EXSTYLE, style | WS_EX_LAYERED | WS_EX_TRANSPARENT);
        else
            // WS_EX_LAYERED는 투명도 유지를 위해 반드시 남겨둬야 합니다.
            SetWindowLong(_hWnd, GWL_EXSTYLE, (style | WS_EX_LAYERED) & ~WS_EX_TRANSPARENT);
        #endif
    }

    public void SetAlwaysOnTop()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        SetWindowPos(_hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        #endif
    }

    public RECT GetActiveWindowRect(out IntPtr activeHWnd)
    {
        activeHWnd = GetForegroundWindow();
        RECT rect = new RECT();
        GetWindowRect(activeHWnd, out rect);
        return rect;
    }
    public Vector3 GetMousePosition()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        POINT pt;
        if (GetCursorPos(out pt))
        {
            // 전역 좌표(스크린)를 클라이언트 좌표(유니티 윈도우 내부)로 변환
            ScreenToClient(_hWnd, ref pt);
            // 유니티는 좌측 하단이 (0,0)이고 마우스 API는 좌측 상단이 (0,0)이므로 Y축 보정
            return new Vector3(pt.X, Screen.height - pt.Y, 0);
        }
        #endif
        return Input.mousePosition;
    }
}
