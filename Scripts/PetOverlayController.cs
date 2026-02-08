using UnityEngine;

public class PetOverlayController : MonoBehaviour
{
    public bool isClickThrough = true;

    void Start()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        Win32Bridge.Instance.SetTransparency(true);
        Win32Bridge.Instance.SetAlwaysOnTop();
        UpdateClickThrough();
        #endif
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
