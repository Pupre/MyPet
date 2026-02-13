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

        if (info == null) return;

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
        // 2D 모드에서는 이 스크립트가 붙은 오브젝트의 SpriteRenderer 사용
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null) _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        // 간단한 스프라이트 애니메이션 세팅 (보통 Idle부터)
        UpdateSpriteSheet(info.idleSpriteSheet, info.frameCount);
        
        transform.localScale = Vector3.one * info.baseScale;
        Debug.Log($"[Visual] {petDefinition.speciesName} {info.stageNumber}단계 2D 스프라이트로 설정되었습니다.");
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
        // 2D 애니메이션 업데이트
        if (petDefinition != null && petDefinition.GetStageInfo(PetGrowthController.Instance.currentData.currentStage).is2D)
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

        // 스프라이트 시트 자동 슬라이싱 (간단한 버전: 가로로 등분)
        _currentFrames = new Sprite[frames];
        int frameWidth = sheet.width / frames;
        for (int i = 0; i < frames; i++)
        {
            _currentFrames[i] = Sprite.Create(sheet, new Rect(i * frameWidth, 0, frameWidth, sheet.height), new Vector2(0.5f, 0.5f), 100f);
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
