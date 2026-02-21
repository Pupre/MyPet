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

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct NOTIFYICONDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uID;
        public int uFlags;
        public int uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public int dwState;
        public int dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public int uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public int dwInfoFlags;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    public static extern bool Shell_NotifyIcon(int dwMessage, [In] ref NOTIFYICONDATA lpData);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public const int GWL_EXSTYLE = -20;
    public const int GWL_STYLE = -16;
    public const uint WS_EX_LAYERED = 0x00080000;
    public const uint WS_EX_TRANSPARENT = 0x00000020;
    public const uint WS_EX_TOOLWINDOW = 0x00000080; // 작업표시줄 숨기기
    public const uint LWA_COLORKEY = 0x00000001;
    public const uint LWA_ALPHA = 0x00000002;
    public const uint WS_POPUP = 0x80000000;
    public const uint WS_VISIBLE = 0x10000000;

    public const int NIM_ADD = 0x00;
    public const int NIM_MODIFY = 0x01;
    public const int NIM_DELETE = 0x02;
    public const int NIF_MESSAGE = 0x01;
    public const int NIF_ICON = 0x02;
    public const int NIF_TIP = 0x04;
    public const int WM_USER = 0x0400;
    public const int WM_TRAYICON = WM_USER + 1;

    public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_SHOWWINDOW = 0x0040;

    public const int SW_HIDE = 0;
    public const int SW_SHOW = 5;

    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_WIN = 0x0008;

    private IntPtr _hWnd;
    private const int HOTKEY_ID = 9000;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        _hWnd = GetActiveWindow();
        #endif
    }

    public void RegisterGlobalHotkey(uint modifiers, uint key)
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        RegisterHotKey(_hWnd, HOTKEY_ID, modifiers, key);
        #endif
    }

    public void UnregisterGlobalHotkey()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        UnregisterHotKey(_hWnd, HOTKEY_ID);
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
    public const uint SWP_FRAMECHANGED = 0x0020;

    public void SetTaskbarIconVisible(bool visible)
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        if (_hWnd == IntPtr.Zero) _hWnd = GetActiveWindow();
        if (_hWnd == IntPtr.Zero) _hWnd = GetForegroundWindow(); // Fallback
        
        uint exStyle = GetWindowLong(_hWnd, GWL_EXSTYLE);
        if (visible)
            SetWindowLong(_hWnd, GWL_EXSTYLE, exStyle & ~WS_EX_TOOLWINDOW);
        else
            SetWindowLong(_hWnd, GWL_EXSTYLE, exStyle | WS_EX_TOOLWINDOW);

        // 프레임 변경 알림 및 스타일 갱신 강제화
        SetWindowPos(_hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        #endif
    }

    public const uint SWP_NOZORDER = 0x0004;

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
