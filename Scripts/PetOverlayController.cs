using UnityEngine;

public class PetOverlayController : MonoBehaviour
{
    public bool isClickThrough = true;

    void Start()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        // 순서가 중요할 수 있음
        Win32Bridge.Instance.SetTransparency(true);
        Win32Bridge.Instance.SetAlwaysOnTop();
        UpdateClickThrough();
        
        // 투명화가 즉시 적용되지 않을 경우를 대비해 약간의 지연 후 재호출하거나 강제 호출
        Invoke(nameof(ForceApplySettings), 0.1f);
        #endif
    }

    void ForceApplySettings()
    {
        Win32Bridge.Instance.SetTransparency(true);
        UpdateClickThrough();
    }

    public void UpdateClickThrough()
    {
        Win32Bridge.Instance.SetClickThrough(isClickThrough);
    }

    // 나중에 펫과 상호작용 할 때 호출할 함수 예시
    public void SetInteractionFocus(bool focus)
    {
        isClickThrough = !focus;
        UpdateClickThrough();
    }
}
