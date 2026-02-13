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
        // 1. 필요한 인스턴스들이 있는지 먼저 확인 (Null 체크)
        if (petDefinition == null || PetGrowthController.Instance == null || PetGrowthController.Instance.currentData == null) 
            return;

        // 2. 현재 단계 정보 가져오기
        PetStageInfo info = petDefinition.GetStageInfo(PetGrowthController.Instance.currentData.currentStage);
        
        // 3. 단계 정보가 있고 2D 모드일 때만 애니메이션 업데이트
        if (info != null && info.is2D)
        {
            Update2DAnimation();
        }
    }

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

        // 스프라이트 시트 자동 슬라이싱 (간단한 버전: 가로로 등분)
        _currentFrames = new Sprite[frames];
        int frameWidth = Mathf.Max(1, sheet.width / frames);
        
        for (int i = 0; i < frames; i++)
        {
            float xPos = i * frameWidth;
            // 텍스처 범위를 벗어나지 않도록 보정
            if (xPos + frameWidth > sheet.width) frameWidth = sheet.width - (int)xPos;
            if (frameWidth <= 0) break;

            _currentFrames[i] = Sprite.Create(sheet, new Rect(xPos, 0, frameWidth, sheet.height), new Vector2(0.5f, 0.5f), 100f);
            _currentFrames[i].name = $"{sheet.name}_{i}";
        }
        _frameIndex = 0;
        _frameTimer = 0f;
    }

    // 애니메이션 상태를 변경하기 위한 인터페이스
    public void SetAnimationState(string paramName, bool value)
    {
        // 3D 애니메이터 제어
        if (_animator != null) _animator.SetBool(paramName, value);

        // 2D 스프라이트 시트 교체 (isMoving 기준)
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
    }
}
