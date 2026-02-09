using System;

[Serializable]
public class PetData
{
    public string petName = "New Pet";
    public int currentStage = 0; // 0~5단계
    public float growthProgress = 0f; // 현재 단계에서의 성장률 (0~100)
    public int affectionLevel = 0; // 애정도 (내부적 수치)
    
    public string lastFeedTimeStr; // 마지막으로 밥 준 시간 (DateTime.ToString())

    public DateTime LastFeedTime
    {
        get => string.IsNullOrEmpty(lastFeedTimeStr) ? DateTime.MinValue : DateTime.Parse(lastFeedTimeStr);
        set => lastFeedTimeStr = value.ToString();
    }

    // 초기 데이터 생성을 위한 생성자
    public PetData()
    {
        petName = "Unnamed Pet";
        currentStage = 0;
        growthProgress = 0f;
        affectionLevel = 0;
        LastFeedTime = DateTime.MinValue;
    }
}
