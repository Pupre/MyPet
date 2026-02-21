using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class TrayIconManager : MonoBehaviour
{
    public static TrayIconManager Instance { get; private set; }

    [Header("Tray Settings")]
    public string trayTooltip = "MyPet - 귀여운 바탕화면 친구";
    public bool hideTaskbarIconOnStart = true;

    private Win32Bridge.NOTIFYICONDATA _nid;
    private bool _isTrayIconCreated = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        CreateTrayIcon();
        if (hideTaskbarIconOnStart)
        {
            SetTaskbarVisible(false);
        }
        #else
        Debug.Log("[Tray] 에디터 모드입니다. 트레이 기능과 작업표시줄 숨기기는 빌드 후 .exe에서만 작동합니다.");
        #endif
    }

    void OnApplicationQuit()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        RemoveTrayIcon();
        #endif
    }

    public void CreateTrayIcon()
    {
        IntPtr handle = Win32Bridge.GetActiveWindow();
        if (handle == IntPtr.Zero) handle = Win32Bridge.GetForegroundWindow();

        Debug.Log($"[Tray] Attempting to create tray icon for handle: {handle}");

        _nid = new Win32Bridge.NOTIFYICONDATA();
        _nid.cbSize = Marshal.SizeOf(typeof(Win32Bridge.NOTIFYICONDATA));
        _nid.hWnd = handle;
        _nid.uID = 1;
        _nid.uFlags = Win32Bridge.NIF_ICON | Win32Bridge.NIF_TIP | Win32Bridge.NIF_MESSAGE;
        _nid.uCallbackMessage = Win32Bridge.WM_TRAYICON;
        _nid.hIcon = GetAppIcon();
        _nid.szTip = trayTooltip;

        _isTrayIconCreated = Win32Bridge.Shell_NotifyIcon(Win32Bridge.NIM_ADD, ref _nid);
        
        if (_isTrayIconCreated)
            Debug.Log("[Tray] 시스템 트레이 아이콘이 생성되었습니다.");
        else
        {
            int error = Marshal.GetLastWin32Error();
            Debug.LogError($"[Tray] 트레이 아이콘 생성 실패! Error Code: {error}");
        }
    }

    public void RemoveTrayIcon()
    {
        if (_isTrayIconCreated)
        {
            Win32Bridge.Shell_NotifyIcon(Win32Bridge.NIM_DELETE, ref _nid);
            _isTrayIconCreated = false;
        }
    }

    public const int NIF_INFO = 0x10;

    public void ShowNotification(string title, string message)
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        _nid.uFlags |= NIF_INFO;
        _nid.szInfoTitle = title;
        _nid.szInfo = message;
        _nid.dwInfoFlags = 1; // 1: Info, 2: Warning, 3: Error
        
        Win32Bridge.Shell_NotifyIcon(Win32Bridge.NIM_MODIFY, ref _nid);
        #endif
    }

    public void SetTaskbarVisible(bool visible)
    {
        if (Win32Bridge.Instance != null)
        {
            Win32Bridge.Instance.SetTaskbarIconVisible(visible);
        }
    }

    private IntPtr GetAppIcon()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        // 윈도우 기본 애플리케이션 아이콘 (IDI_APPLICATION = 32512) 로드
        return Win32Bridge.LoadIcon(IntPtr.Zero, (IntPtr)32512);
        #else
        return IntPtr.Zero;
        #endif
    }

    // 클릭 통과(잠금) 기능을 토글하는 범용 메서드
    public void ToggleClickThrough(bool enabled)
    {
        if (Win32Bridge.Instance != null)
        {
            Win32Bridge.Instance.SetClickThrough(enabled);
            Debug.Log($"[Control] 펫 선택 잠금 상태: {enabled}");
        }
    }
}
