using UnityEngine;
using System;

public class PetGrowthController : MonoBehaviour
{
    public static PetGrowthController Instance { get; private set; }

    public PetData currentData;
    public float growthRatePerFeed = 20f; // 밥 한 번에 쌓이는 성장률 (%)
    
    [Header("Visual Settings")]
    public Transform petModel;
    public float stage0Scale = 0.1f;
    public float stage5Scale = 1.0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadPetData();
    }

    void Start()
    {
        ApplyGrowthVisuals();
    }

    public void LoadPetData()
    {
        currentData = SaveManager.Load();
    }

    public void SavePetData()
    {
        SaveManager.Save(currentData);
    }

    // 밥 주기 시도
    public bool TryFeed()
    {
        DateTime now = DateTime.Now;
        TimeSpan elapsed = now - currentData.LastFeedTime;

        // 24시간 체크
        if (elapsed.TotalHours >= 24)
        {
            currentData.LastFeedTime = now;
            AddGrowth(growthRatePerFeed);
            Debug.Log("펫에게 밥을 주었습니다! 성장이 진행됩니다.");
            return true;
        }

        Debug.Log($"아직 배가 부릅니다. 다음 식사 가능 시간까지: {24 - elapsed.TotalHours:F1}시간");
        return false;
    }

    private void AddGrowth(float amount)
    {
        currentData.growthProgress += amount;

        if (currentData.growthProgress >= 100f && currentData.currentStage < 5)
        {
            LevelUp();
        }

        ApplyGrowthVisuals();
        SavePetData();
    }

    private void LevelUp()
    {
        currentData.currentStage++;
        currentData.growthProgress = 0f;
        Debug.Log($"축하합니다! 펫이 {currentData.currentStage}단계로 성장했습니다!");
    }

    public void ApplyGrowthVisuals()
    {
        if (petModel == null) petModel = this.transform;

        // 단계에 따른 스케일 계산 (점진적 성장)
        // 0단계 -> stage0Scale, 5단계 -> stage5Scale
        float t = (float)currentData.currentStage / 5f;
        float targetScale = Mathf.Lerp(stage0Scale, stage5Scale, t);
        
        // 현재 단계 내부에서의 미세한 성장 반영 (Progress)
        float progressBonus = (currentData.growthProgress / 100f) * ((stage5Scale - stage0Scale) / 5f);
        
        petModel.localScale = Vector3.one * (targetScale + progressBonus);

        // TODO: 나중에 단계별 메시(Mesh) 교체 로직 추가
    }

    // 테스트용: 강제 성장 버튼 등에서 호출
    [ContextMenu("Debug Feed")]
    public void DebugFeed()
    {
        currentData.LastFeedTime = DateTime.MinValue; // 쿨타임 초기화
        TryFeed();
    }
}
