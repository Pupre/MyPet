using UnityEngine;

public class PetOverlayController : MonoBehaviour
{
    public bool isClickThrough = true;

    void Start()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        // 1. 카메라 배경 설정 강제화 (실수 방지)
        var cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0); // 완전 투명한 검은색
            cam.allowHDR = false; // HDR 강제 비활성화
        }

        // 2. Win32 API 호출 (순서 중요)
        Win32Bridge.Instance.SetTransparency(true);
        
        // 크로마키 적용: 검은색(0,0,0)을 투명하게 처리
        // DwmExtend...가 실패하거나 URP가 검은색을 뱉을 때를 대비한 안전장치
        Win32Bridge.Instance.SetColorKey(new Color32(0, 0, 0, 255));
        
        Win32Bridge.Instance.SetAlwaysOnTop();
        UpdateClickThrough();
        
        // 타이밍 이슈 대비: 잠시 후 재적용
        Invoke(nameof(ForceApplySettings), 0.1f);
        #endif
    }

    void Update()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        DetectMouseInteraction();
        #endif
    }

    void DetectMouseInteraction()
    {
        // 마우스 위치로 레이캐스트
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 펫(Collider가 있는 객체)이 마우스 아래에 있는지 확인
        bool isHovering = Physics.Raycast(ray, out hit, 100f);

        // 상태가 바뀔 때만 API 호출 (성능 최적화)
        if (isHovering && isClickThrough)
        {
            isClickThrough = false; // 상호작용 모드 (클릭 받음)
            UpdateClickThrough();
        }
        else if (!isHovering && !isClickThrough)
        {
            isClickThrough = true; // 통과 모드 (배경 클릭 가능)
            UpdateClickThrough();
        }

        // --- 롱프레스 및 방사형 메뉴 연동 ---
        if (isHovering && Input.GetMouseButtonDown(0))
        {
            if (RadialMenuController.Instance != null)
                RadialMenuController.Instance.StartLongPress();
        }
        
        if (Input.GetMouseButton(0))
        {
            if (RadialMenuController.Instance != null)
                RadialMenuController.Instance.UpdateLongPress(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (RadialMenuController.Instance != null)
                RadialMenuController.Instance.CancelLongPress();
        }
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
