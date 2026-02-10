using UnityEngine;
using System;

public class PetGrowthController : MonoBehaviour
{
    public static PetGrowthController Instance { get; private set; }

    public PetData currentData;
    public float growthRatePerFeed = 20f; // 밥 한 번에 쌓이는 성장률 (%)
    
    [Header("Visual Settings")]
    public Transform petModel;
    public float stage0Scale = 1.5f; // 테스트를 위해 더 상향 (1.0 -> 1.5)
    public float stage5Scale = 3.0f;

    [Header("Time Settings")]
    public int resetHour = 6; // 매일 오전 6시 초기화

    [Header("Time Settings")]
    public int resetHour = 6; // 매일 오전 6시 초기화

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
        
        // 현재 시각 기준 가장 최근의 오전 6시 구하기
        DateTime lastResetTime = new DateTime(now.Year, now.Month, now.Day, resetHour, 0, 0);
        if (now.Hour < resetHour)
        {
            lastResetTime = lastResetTime.AddDays(-1);
        }

        // 마지막 밥 준 시간이 최근 초기화 시간보다 이전이면 밥 주기 가능
        if (currentData.LastFeedTime < lastResetTime)
        {
            currentData.LastFeedTime = now;
            AddGrowth(growthRatePerFeed);
            Debug.Log("펫에게 밥을 주었습니다! 성장이 진행됩니다.");
            return true;
        }

        // 다음에 밥 줄 수 있는 시간 계산
        DateTime nextResetTime = lastResetTime.AddDays(1);
        TimeSpan waitTime = nextResetTime - now;
        Debug.Log($"이미 밥을 먹었습니다. 오전 {resetHour}시 초기화까지 {waitTime.Hours}시간 {waitTime.Minutes}분 남았습니다.");
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
        float finalScale = targetScale + progressBonus;
        petModel.localScale = Vector3.one * finalScale;

        Debug.Log($"[Scale Debug] 현재 단계: {currentData.currentStage}, 적용 스케일: {finalScale}");

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
