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
        #if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        }
        #endif
        #else
        mousePos = Win32Bridge.Instance.GetMousePosition();
        #endif

        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        bool isHovering = Physics.Raycast(ray, out hit, 100f);
        
        // 시각적 확인을 위한 레이 (조금 더 연하게 유지)
        Debug.DrawRay(ray.origin, ray.direction * 100f, isHovering ? Color.green : new Color(1, 0, 0, 0.2f));

        // 상태가 바뀔 때만 API 호출 (롱프레스 중에는 클릭 통과 모드 전환 방지)
        bool isMouseHeld = Input.GetMouseButton(0);
        #if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null) isMouseHeld = UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
        #endif

        if (isHovering && isClickThrough)
        {
            isClickThrough = false;
            #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            UpdateClickThrough();
            #endif
        }
        else if (!isHovering && !isClickThrough && !isMouseHeld) // 마우스를 떼고 나서만 클릭 통과 모드 복구
        {
            isClickThrough = true;
            #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            UpdateClickThrough();
            #endif
        }

        // --- 롱프레스 및 방사형 메뉴 연동 ---
        bool isMouseDown = Input.GetMouseButtonDown(0);
        bool isMouseUp = Input.GetMouseButtonUp(0);
        #if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null) {
            isMouseDown = UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;
            isMouseUp = UnityEngine.InputSystem.Mouse.current.leftButton.wasReleasedThisFrame;
        }
        #endif

        if (isHovering && isMouseDown)
        {
            if (RadialMenuController.Instance != null)
                RadialMenuController.Instance.StartLongPress();

            if (_petMovement != null) _petMovement.isLocked = true;
        }
        
        if (isMouseHeld)
        {
            if (RadialMenuController.Instance != null)
                RadialMenuController.Instance.UpdateLongPress(mousePos); // 보정된 좌표 전달
        }

        if (isMouseUp)
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
