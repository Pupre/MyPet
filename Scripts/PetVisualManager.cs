using UnityEngine;

public class PetVisualManager : MonoBehaviour
{
    public static PetVisualManager Instance { get; private set; }

    public PetDefinition petDefinition;
    private GameObject _currentModel;
    private Animator _animator;

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

        if (info != null && info.modelPrefab != null)
        {
            // 기존 모델 제거
            if (_currentModel != null) Destroy(_currentModel);

            // 새로운 모델 생성
            _currentModel = Instantiate(info.modelPrefab, transform);
            _currentModel.transform.localPosition = Vector3.zero;
            _currentModel.transform.localRotation = Quaternion.identity;
            
            // 데이터 기반 기본 스케일 설정
            _currentModel.transform.localScale = Vector3.one * info.baseScale;
            
            // 애니메이터 설정
            _animator = _currentModel.GetComponent<Animator>();
            if (_animator != null && info.animatorController != null)
            {
                _animator.runtimeAnimatorController = info.animatorController;
            }

            Debug.Log($"[Visual] {petDefinition.speciesName} {currentStage}단계 모델로 교체되었습니다.");
        }
    }

    // 애니메이션 상태를 변경하기 위한 인터페이스
    public void SetAnimationState(string paramName, bool value)
    {
        if (_animator != null) _animator.SetBool(paramName, value);
    }

    public void TriggerAnimation(string triggerName)
    {
        if (_animator != null) _animator.SetTrigger(triggerName);
    }
}
