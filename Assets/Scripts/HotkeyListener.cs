using UnityEngine;
using System;

public class HotkeyListener : MonoBehaviour
{
    private bool _isLocked = false;
    private float _lastToggleTime = 0f;
    private const float Cooldown = 0.5f;

    void Update()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        // Ctrl + Shift + L 키 조합 감지
        bool ctrl = (Win32Bridge.GetAsyncKeyState(0x11) & 0x8000) != 0; // VK_CONTROL
        bool shift = (Win32Bridge.GetAsyncKeyState(0x10) & 0x8000) != 0; // VK_SHIFT
        bool lKey = (Win32Bridge.GetAsyncKeyState(0x4C) & 0x8000) != 0; // 'L' Key (Lock)

        if (ctrl && shift && lKey && Time.unscaledTime > _lastToggleTime + Cooldown)
        {
            _lastToggleTime = Time.unscaledTime;
            ToggleLock();
        }
        #endif
    }

    private void ToggleLock()
    {
        _isLocked = !_isLocked;
        
        if (TrayIconManager.Instance != null)
        {
            TrayIconManager.Instance.ToggleClickThrough(_isLocked);
            
            string title = _isLocked ? "펫 선택 잠금 활성화" : "펫 선택 잠금 해제";
            string msg = _isLocked ? "이제 작업 중에 펫이 클릭되지 않습니다. (다시 풀려면 Ctrl+Shift+L)" : "다시 펫과 상호작용할 수 있습니다.";
            TrayIconManager.Instance.ShowNotification(title, msg);
        }
    }
}
