using UnityEngine;
using System;

public class PetNeedsController : MonoBehaviour
{
    public static PetNeedsController Instance { get; private set; }

    [Header("Hunger Settings")]
    public float hungerDepletionPerHour = 4.0f; // 1시간에 4% 감소 (약 25시간이면 0)
    public float testDepletionMultiplier = 1.0f; // 테스트용 배속

    private PetGrowthController _growthController;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        _growthController = PetGrowthController.Instance;
        CalculateOfflineDepletion();
    }

    void Update()
    {
        UpdateHungerRealtime();
    }

    // 앱이 꺼져있을 때 얼마나 배고파졌는지 계산
    private void CalculateOfflineDepletion()
    {
        if (_growthController == null || _growthController.currentData == null) return;

        DateTime lastUpdate = _growthController.currentData.LastUpdateTime;
        DateTime now = DateTime.Now;

        TimeSpan passed = now - lastUpdate;
        double hoursPassed = passed.TotalHours;

        if (hoursPassed > 0)
        {
            float totalDepletion = (float)(hoursPassed * hungerDepletionPerHour * testDepletionMultiplier);
            _growthController.currentData.hunger = Mathf.Max(0, _growthController.currentData.hunger - totalDepletion);
            _growthController.currentData.LastUpdateTime = now;
            
            Debug.Log($"[Needs] 오프라인 동안 {hoursPassed:F2}시간이 지났습니다. 허기 {totalDepletion:F2}% 감소. 현재 허기: {_growthController.currentData.hunger:F2}%");
            _growthController.SavePetData();
        }
    }

    // 실시간 허기 감소
    private void UpdateHungerRealtime()
    {
        if (_growthController == null || _growthController.currentData == null) return;

        float depletionPerSecond = (hungerDepletionPerHour * testDepletionMultiplier) / 3600f;
        _growthController.currentData.hunger -= depletionPerSecond * Time.deltaTime;
        
        // 최소값 고정
        if (_growthController.currentData.hunger < 0) _growthController.currentData.hunger = 0;

        // 1분마다 자동 저장 (혹은 크리티컬한 시점)
        if (Time.frameCount % 3600 == 0) // 약 60초 (60fps 기준)
        {
            _growthController.currentData.LastUpdateTime = DateTime.Now;
            _growthController.SavePetData();
        }
    }

    public void RestoreHunger(float amount)
    {
        if (_growthController == null || _growthController.currentData == null) return;

        _growthController.currentData.hunger = Mathf.Min(100f, _growthController.currentData.hunger + amount);
        _growthController.currentData.LastUpdateTime = DateTime.Now;
        _growthController.SavePetData();
        
        Debug.Log($"[Needs] 밥을 먹었습니다! 현재 허기: {_growthController.currentData.hunger:F2}%");
    }

    public float GetHungerNormalized()
    {
        if (_growthController == null || _growthController.currentData == null) return 1f;
        return _growthController.currentData.hunger / 100f;
    }
}
