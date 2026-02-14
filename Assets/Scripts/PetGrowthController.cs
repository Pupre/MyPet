using UnityEngine;
using System;

public class PetGrowthController : MonoBehaviour
{
    public static PetGrowthController Instance { get; private set; }

    public PetData currentData;
    
    // 이펙터 수정을 방지하기 위해 상수로 하드코딩
    private const float GrowthRatePerFeed = 100f; // 밥 한 번에 100% 성장 (테스트 모드)
    private const float Stage0Scale = 3.0f; // 시작 크기 조정 (0.5 -> 3.0)
    private const float Stage5Scale = 7.0f; // 최대 크기 조정 (5.0 -> 7.0)
    private const int ResetHour = 6; // 매일 오전 6시 초기화 (KST)

    [Header("Visual Settings")]
    public Transform petModel;

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
        DateTime lastResetTime = new DateTime(now.Year, now.Month, now.Day, ResetHour, 0, 0);
        if (now.Hour < ResetHour)
        {
            lastResetTime = lastResetTime.AddDays(-1);
        }

        // 마지막 밥 준 시간이 최근 초기화 시간보다 이전이면 밥 주기 가능
        if (currentData.LastFeedTime < lastResetTime)
        {
            currentData.LastFeedTime = now;
            AddGrowth(GrowthRatePerFeed);
            Debug.Log("펫에게 밥을 주었습니다! 성장이 진행됩니다.");
            return true;
        }

        // 다음에 밥 줄 수 있는 시간 계산 (디버그용)
        DateTime nextResetTime = lastResetTime.AddDays(1);
        TimeSpan waitTime = nextResetTime - now;
        Debug.Log($"이미 밥을 먹었습니다. 오전 {ResetHour}시 초기화까지 {waitTime.Hours}시간 {waitTime.Minutes}분 남았습니다.");
        return false;
    }

    private void AddGrowth(float amount)
    {
        currentData.growthProgress += amount;

        // 성장률이 100%를 넘으면 단계 상승
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
        // 1. 비주얼 매니저에게 모델 갱신 요청 (새로운 시스템)
        if (PetVisualManager.Instance != null)
        {
            PetVisualManager.Instance.RefreshVisuals();
        }

        if (petModel == null) petModel = this.transform;

        // 2. 현재 단계(Stage)와 진행도에 따른 미세 스케일 조정 (기존 로직 유지)
        float t = (float)currentData.currentStage / 5f;
        float baseScale = Mathf.Lerp(Stage0Scale, Stage5Scale, t);
        float scaleStep = (Stage5Scale - Stage0Scale) / 5f;
        float progressBonus = (currentData.growthProgress / 100f) * scaleStep;
        
        float finalScale = baseScale + progressBonus;
        petModel.localScale = Vector3.one * finalScale;

        Debug.Log($"[성장 로그] 현재 단계: {currentData.currentStage}, 성장률: {currentData.growthProgress}%, 최종 스케일: {finalScale}");

        // TODO: 나중에 단계별 메시(Mesh) 교체 로직 추가
    }

    [ContextMenu("Debug Feed")]
    public void DebugFeed()
    {
        currentData.LastFeedTime = DateTime.MinValue; 
        TryFeed();
    }
}
