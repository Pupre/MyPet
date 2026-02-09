using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RadialMenuController : MonoBehaviour
{
    public static RadialMenuController Instance { get; private set; }

    [Header("UI References")]
    public GameObject menuRoot;
    public Image progressGauge; // 원형 Fill 이미지
    public GameObject feedButton; // 밥 주기 아이콘/버튼

    [Header("Settings")]
    public float requiredHoldTime = 1.0f;
    private float _currentHoldTime = 0f;
    private bool _isMenuOpen = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        menuRoot.SetActive(false);
        progressGauge.fillAmount = 0;
    }

    public void StartLongPress()
    {
        if (_isMenuOpen) return;
        _currentHoldTime = 0f;
        progressGauge.gameObject.SetActive(true);
    }

    public void UpdateLongPress(Vector3 mousePos)
    {
        if (_isMenuOpen) return;

        _currentHoldTime += Time.deltaTime;
        progressGauge.fillAmount = _currentHoldTime / requiredHoldTime;

        // UI 위치를 마우스나 펫 위치로 업데이트 (World to Screen 변환 필요할 수 있음)
        progressGauge.transform.position = mousePos;

        if (_currentHoldTime >= requiredHoldTime)
        {
            OpenMenu(mousePos);
        }
    }

    public void CancelLongPress()
    {
        if (_isMenuOpen) return;
        _currentHoldTime = 0f;
        progressGauge.fillAmount = 0;
        progressGauge.gameObject.SetActive(false);
    }

    void OpenMenu(Vector3 position)
    {
        _isMenuOpen = true;
        menuRoot.SetActive(true);
        menuRoot.transform.position = position;
        progressGauge.gameObject.SetActive(false);
        
        Debug.Log("방사형 메뉴가 열렸습니다.");
    }

    public void CloseMenu()
    {
        _isMenuOpen = false;
        menuRoot.SetActive(false);
        _currentHoldTime = 0f;
    }

    // 버튼에서 호출할 함수 (예: 밥 주기)
    public void OnClickFeed()
    {
        if (PetGrowthController.Instance != null)
        {
            PetGrowthController.Instance.TryFeed();
        }
        CloseMenu();
    }
}
