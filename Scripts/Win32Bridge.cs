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
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

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
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    public const int GWL_EXSTYLE = -20;
    public const uint WS_EX_LAYERED = 0x00080000;
    public const uint WS_EX_TRANSPARENT = 0x00000020;
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
            MARGINS margins = new MARGINS { cxLeftWidth = -1 };
            DwmExtendFrameIntoClientArea(_hWnd, ref margins);
        }
        #endif
    }

    public void SetClickThrough(bool enabled)
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        uint style = GetWindowLong(_hWnd, GWL_EXSTYLE);
        if (enabled)
            SetWindowLong(_hWnd, GWL_EXSTYLE, style | WS_EX_LAYERED | WS_EX_TRANSPARENT);
        else
            SetWindowLong(_hWnd, GWL_EXSTYLE, style & ~(WS_EX_LAYERED | WS_EX_TRANSPARENT));
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
}
