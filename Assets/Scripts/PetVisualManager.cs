using UnityEngine;

public class PetVisualManager : MonoBehaviour
{
    public static PetVisualManager Instance { get; private set; }

    public PetDefinition petDefinition;
    private GameObject _currentModel;
    private Animator _animator;
    
    [Header("2D Support")]
    private SpriteRenderer _spriteRenderer;
    private Sprite[] _currentFrames;
    private int _frameIndex;
    private float _frameTimer;
    private float _frameDuration = 0.15f; // 150ms per frame

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        RefreshVisuals();
    }

    /// <summary>
    /// 현재 PetData의 단계에 맞춰 모델과 애니메이터를 새로고침합니다.
    /// </summary>
    public void RefreshVisuals()
    {
        if (PetGrowthController.Instance == null || petDefinition == null) return;

        int currentStage = PetGrowthController.Instance.currentData.currentStage;
        PetStageInfo info = petDefinition.GetStageInfo(currentStage);

        Debug.Log($"[Visual Debug] 갱신 시도 - 현재 단계: {currentStage}, 찾은 정보 있음: {info != null}");

        if (info == null)
        {
            Debug.LogWarning($"[Visual Debug] 펫 정의에서 {currentStage}단계 정보를 찾을 수 없습니다! Stages 리스트에 Stage Number가 {currentStage}인 항목이 있는지 확인해 주세요.");
            return;
        }

        // 기존 모델 제거
        if (_currentModel != null) Destroy(_currentModel);

        if (info.is2D)
        {
            Setup2DVisuals(info);
        }
        else if (info.modelPrefab != null)
        {
            Setup3DVisuals(info, currentStage);
        }
    }

    private void Setup2DVisuals(PetStageInfo info)
    {
        if (info == null) return;

        // 2D 비주얼을 전용 자식 오브젝트로 생성 (기존 컴포넌트와 충돌 방지)
        _currentModel = new GameObject("2D Visual");
        _currentModel.transform.SetParent(transform);
        _currentModel.transform.localPosition = Vector3.zero;
        _currentModel.transform.localRotation = Quaternion.identity;
        _currentModel.transform.localScale = Vector3.one * info.baseScale;

        _spriteRenderer = _currentModel.AddComponent<SpriteRenderer>();
        
        if (_spriteRenderer == null)
        {
            Debug.LogError("[Visual Debug] 2D 비주얼 오브젝트에 SpriteRenderer를 추가하는데 실패했습니다!");
            return;
        }

        _spriteRenderer.enabled = true;
        _spriteRenderer.color = Color.white;
        _spriteRenderer.sortingOrder = 10; 

        // 간단한 스프라이트 애니메이션 세팅
        if (info.idleSpriteSheet != null)
        {
            int frames = Mathf.Max(1, info.frameCount);
            UpdateSpriteSheet(info.idleSpriteSheet, frames);
        }
        else
        {
            Debug.LogWarning($"[Visual Debug] {info.stageNumber}단계의 Idle Sprite Sheet가 비어있습니다!");
        }

        Debug.Log($"[Visual] {petDefinition.speciesName} {info.stageNumber}단계 2D 비주얼이 생성되었습니다.");
    }

    private void Setup3DVisuals(PetStageInfo info, int currentStage)
    {
        if (_spriteRenderer != null) _spriteRenderer.enabled = false;

        _currentModel = Instantiate(info.modelPrefab, transform);
        _currentModel.transform.localPosition = Vector3.zero;
        _currentModel.transform.localRotation = Quaternion.identity;
        _currentModel.transform.localScale = Vector3.one * info.baseScale;
        
        _animator = _currentModel.GetComponent<Animator>();
        if (_animator != null && info.animatorController != null)
        {
            _animator.runtimeAnimatorController = info.animatorController;
        }

        Debug.Log($"[Visual] {petDefinition.speciesName} {currentStage}단계 3D 모델로 교체되었습니다.");
    }

    void Update()
    {
        if (petDefinition == null || PetGrowthController.Instance == null || PetGrowthController.Instance.currentData == null) 
            return;

        PetStageInfo info = petDefinition.GetStageInfo(PetGrowthController.Instance.currentData.currentStage);
        
        if (info != null && info.is2D)
        {
            Update2DAnimation();
            Update2DFlip();
        }
    }

    private void Update2DFlip()
    {
        if (_spriteRenderer == null) return;

        // 펫이 움직이고 있다면 진행 방향에 따라 좌우 반전
        // 여기서 transform.parent는 펫의 실제 이동 주체(PetMovement가 붙은 오브젝트)임
        if (PetStateMachine.Instance != null && PetStateMachine.Instance.GetCurrentState() == PetState.Move)
        {
            // PetMovement의 위치 변화를 직접 감시해서 방향 결정
            // (localScale을 뒤집지 않고 spriteRenderer.flipX를 사용하면 자식 오브젝트 관리가 편함)
            float moveDirection = transform.position.x - _lastX;
            if (Mathf.Abs(moveDirection) > 0.001f)
            {
                // 로직상 방향이 반대라면 !isMovingRight 등으로 조정 가능
                // 여기서는 오른쪽으로 갈 때 flipX = true (이미지에 따라 다름)
                _spriteRenderer.flipX = moveDirection < 0; 
            }
            _lastX = transform.position.x;
        }
    }
    private float _lastX;

    private void Update2DAnimation()
    {
        if (_currentFrames == null || _currentFrames.Length == 0 || _spriteRenderer == null) return;

        _frameTimer += Time.deltaTime;
        if (_frameTimer >= _frameDuration)
        {
            _frameTimer = 0f;
            _frameIndex = (_frameIndex + 1) % _currentFrames.Length;
            _spriteRenderer.sprite = _currentFrames[_frameIndex];
        }
    }

    public void UpdateSpriteSheet(Texture2D sheet, int frames)
    {
        if (sheet == null) return;
        if (frames <= 0) frames = 1;

        _currentFrames = new Sprite[frames];
        int frameWidth = Mathf.Max(1, sheet.width / frames);
        
        for (int i = 0; i < frames; i++)
        {
            float xPos = i * frameWidth;
            if (xPos + frameWidth > sheet.width) frameWidth = sheet.width - (int)xPos;
            if (frameWidth <= 0) break;

            // 엣지 블리딩 방지를 위해 0.5픽셀 마진을 둡니다.
            float inset = 0.5f;
            _currentFrames[i] = Sprite.Create(sheet, 
                new Rect(xPos + inset, inset, frameWidth - (inset * 2), sheet.height - (inset * 2)), 
                new Vector2(0.5f, 0.5f), 100f);
            _currentFrames[i].name = $"{sheet.name}_{i}";
        }
        _frameIndex = 0;
        _frameTimer = 0f;
    }

    /// <summary>
    /// 이름 기반으로 특정 액션(애니메이션)을 재생합니다. 
    /// 사용자님이 인스펙터에서 추가한 이름을 입력하면 됩니다. (예: "Attack", "Special")
    /// </summary>
    public void PlayAction(string actionName)
    {
        if (petDefinition == null) return;
        PetStageInfo info = petDefinition.GetStageInfo(PetGrowthController.Instance.currentData.currentStage);
        if (info == null) return;

        if (info.is2D)
        {
            // 리스트에서 이름으로 검색
            PetAction action = info.specialActions.Find(a => a.actionName == actionName);
            if (action != null && action.spriteSheet != null)
            {
                UpdateSpriteSheet(action.spriteSheet, action.frameCount);
            }
        }
        else
        {
            // 3D의 경우 애니메이터 트리거로 작동
            if (_animator != null) _animator.SetTrigger(actionName);
        }
    }

    // 애니메이션 상태를 변경하기 위한 인터페이스
    public void SetAnimationState(string paramName, bool value)
    {
        if (_animator != null) _animator.SetBool(paramName, value);

        if (petDefinition != null)
        {
            PetStageInfo info = petDefinition.GetStageInfo(PetGrowthController.Instance.currentData.currentStage);
            if (info != null && info.is2D)
            {
                if (paramName == "isMoving")
                {
                    UpdateSpriteSheet(value ? info.moveSpriteSheet : info.idleSpriteSheet, info.frameCount);
                }
            }
        }
    }

    public void TriggerAnimation(string triggerName)
    {
        if (_animator != null) _animator.SetTrigger(triggerName);
        // 2D에서도 트리거 형태로 재생 원할 경우 PlayAction 호출 가능
        PlayAction(triggerName);
    }
}
