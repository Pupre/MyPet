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

    private PetState _stateBeforeInteraction;
    private float _raycastTimer = 0f;
    private const float RAYCAST_INTERVAL = 0.05f; // 20fps 정도로 레이캐스트 빈도 낮춤 (CPU 절약)
    private bool _lastHoverState = false;

    void Start()
    {
        // CPU 최적화: 프레임 제한 (투명 창 모드에서는 무제한으로 돌아가서 CPU를 많이 먹을 수 있음)
        Application.targetFrameRate = 60;

        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        // 1. 카메라 배경 설정 강제화 (실수 방지)
        // ... (생략 가능하지만 context 유지를 위해 포함)
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

        // --- 레이캐스트 최적화 ---
        _raycastTimer += Time.deltaTime;
        bool isHovering = _lastHoverState;

        if (_raycastTimer >= RAYCAST_INTERVAL)
        {
            _raycastTimer = 0f;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;
            isHovering = Physics.Raycast(ray, out hit, 100f);
            _lastHoverState = isHovering;
            
            // 시각적 확인을 위한 레이
            Debug.DrawRay(ray.origin, ray.direction * 100f, isHovering ? Color.green : new Color(1, 0, 0, 0.2f));
        }

        // --- 상태 체크 ---
        bool isMouseHeld = Input.GetMouseButton(0);
        #if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Mouse.current != null) isMouseHeld = UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
        #endif

        // 클릭 통과 모드 최적화 (상태가 변할 때만 API 호출)
        if (isHovering && isClickThrough)
        {
            isClickThrough = false;
            #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            UpdateClickThrough();
            #endif
        }
        else if (!isHovering && !isClickThrough && !isMouseHeld) 
        {
            isClickThrough = true;
            #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            UpdateClickThrough();
            #endif
        }

        // --- 마우스 다운/업 (롱프레스 및 붙잡기) ---
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
            Debug.Log("[Interaction] Mouse Down Detected on Pet");
            if (PetStateMachine.Instance != null)
                _stateBeforeInteraction = PetStateMachine.Instance.GetCurrentState();

            if (RadialMenuController.Instance != null)
                RadialMenuController.Instance.StartLongPress();

            if (PetStateMachine.Instance != null)
                PetStateMachine.Instance.ChangeState(PetState.Struggling);
            
            if (_petMovement != null) _petMovement.isLocked = true;
        }
        
        if (isMouseHeld)
        {
            if (RadialMenuController.Instance != null)
                RadialMenuController.Instance.UpdateLongPress(mousePos); 
        }

        if (isMouseUp)
        {
            Debug.Log("[Interaction] Mouse Up Detected");
            if (RadialMenuController.Instance != null)
                RadialMenuController.Instance.CancelLongPress();

            if (PetStateMachine.Instance != null)
            {
                // 이전 상태로 복구 (이동 중이었다면 다시 걷기 애니메이션이 나오며 이동 재개)
                PetStateMachine.Instance.ChangeState(_stateBeforeInteraction, false);
            }
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
