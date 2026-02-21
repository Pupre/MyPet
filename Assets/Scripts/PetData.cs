using System;

[Serializable]
public class PetData
{
    public string petID = "0"; // 고유 ID (저장 파일명 구분용)
    public string petName = "New Pet";
    public int currentStage = 0; // 0~5단계
    public float growthProgress = 0f; // 현재 단계에서의 성장률 (0~100)
    public float hunger = 100f; // 배고픔 수치 (0~100)
    public int affectionLevel = 0; // 애정도 (내부적 수치)
    
    public string lastFeedTimeStr; // 마지막으로 밥 준 시간
    public string lastUpdateTimeStr; // 마지막으로 수치가 업데이트된 시간 (방치형 보상/수치 감소용)

    public DateTime LastFeedTime
    {
        get => string.IsNullOrEmpty(lastFeedTimeStr) ? DateTime.MinValue : DateTime.Parse(lastFeedTimeStr);
        set => lastFeedTimeStr = value.ToString();
    }

    public DateTime LastUpdateTime
    {
        get => string.IsNullOrEmpty(lastUpdateTimeStr) ? DateTime.Now : DateTime.Parse(lastUpdateTimeStr);
        set => lastUpdateTimeStr = value.ToString();
    }

    // 초기 데이터 생성을 위한 생성자
    public PetData()
    {
        petName = "Unnamed Pet";
        currentStage = 0;
        growthProgress = 0f;
        hunger = 100f;
        affectionLevel = 0;
        LastFeedTime = DateTime.MinValue;
        LastUpdateTime = DateTime.Now;
    }
}
