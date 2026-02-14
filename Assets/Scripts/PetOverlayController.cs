using UnityEngine;

public class PetOverlayController : MonoBehaviour
{
    public static PetOverlayController Instance { get; private set; }
    public bool isClickThrough = false; // 시작할 때는 클릭을 받도록 설정
    private PetMovement _petMovement;

    void Awake()
    {
        if (Instance == null) Instance = this;
        _petMovement = GetComponent<PetMovement>();
    }

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
        
        // 3. 방사형 메뉴 대상 설정
        if (RadialMenuController.Instance != null)
        {
            RadialMenuController.Instance.targetPet = this.transform;
        }

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
        DetectMouseInteraction();
    }
    void DetectMouseInteraction()
    {
        Vector3 mousePos = Input.mousePosition;

        #if UNITY_EDITOR
        // 신규 인풋 시스템(Both 모드)에서 Input.mousePosition이 엉뚱한 값을 뱉는 경우 대응
        // Pointer.current나 Mouse.current가 있다면 그 값을 우선시합니다.
        #if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        }
        #endif
        #else
        mousePos = Win32Bridge.Instance.GetMousePosition();
        #endif

        if (Camera.main == null) {
            if (Time.frameCount % 60 == 0) Debug.LogError("[PetInteraction] Camera.main is null!");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;

        // 펫(Collider가 있는 객체)이 마우스 아래에 있는지 확인
        // 레이어 마스크를 0(Default)으로 명시하거나 펫의 레이어에 맞게 조정하세요.
        bool isHovering = Physics.Raycast(ray, out hit, 100f);
        
        // 씬 뷰에서 확인 가능한 디버그 선 (거리를 100으로 늘림)
        Debug.DrawRay(ray.origin, ray.direction * 100f, isHovering ? Color.green : Color.red);

        // [핵심 디버그] 레이가 이상한 곳으로 갈 때 정보를 매 프레임 찍지 않고 1초마다 출력
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[PetDebug] Cam: {Camera.main.name}, Screen: ({Screen.width}x{Screen.height}), Mouse: {mousePos}, RayPos: {ray.origin}, RayDir: {ray.direction}, Hover: {isHovering}");
        }

        // 상태가 바뀔 때만 API 호출
        if (isHovering && isClickThrough)
        {
            Debug.Log($"[PetInteraction] SUCCESS: Hover Detected on {hit.collider.name}");
            isClickThrough = false;
            #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            UpdateClickThrough();
            #endif
        }
        else if (!isHovering && !isClickThrough)
        {
            Debug.Log("[PetInteraction] Mouse Left");
            isClickThrough = true;
            #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            UpdateClickThrough();
            #endif
        }

        // --- 롱프레스 및 방사형 메뉴 연동 ---
        if (isHovering && Input.GetMouseButtonDown(0))
        {
            if (RadialMenuController.Instance != null)
                RadialMenuController.Instance.StartLongPress();

            if (_petMovement != null) _petMovement.isLocked = true;
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

            if (_petMovement != null) _petMovement.isLocked = false;
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
