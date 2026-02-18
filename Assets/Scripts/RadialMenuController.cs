using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RadialMenuController : MonoBehaviour
{
    public static RadialMenuController Instance { get; private set; }

    [Header("Target Pet")]
    public Transform targetPet; // 펫의 위치를 추적하기 위해 필요

    [Header("UI References")]
    public GameObject menuRoot;
    public Image progressGauge; // 원형 Fill 이미지
    public GameObject feedButton; // 밥 주기 아이콘/버튼
    public GameObject instantFeedButton; // 즉시 밥 주기 아이콘/버튼
    public GameObject resetButton; // 레벨 초기화 아이콘/버튼

    [Header("Settings")]
    public float requiredHoldTime = 1.0f;
    public float selectionDeadzone = 50f; // 중심에서 이 거리 이상 나가야 선택됨
    
    private float _currentHoldTime = 0f;
    private bool _isMenuOpen = false;
    private Vector3 _menuCenterPos;
    private int _selectedItemIndex = -1; // -1: 없음, 0: 밥주기, 1: 즉시밥주기, 2: 레벨초기화

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
        if (_isMenuOpen)
        {
            UpdateSelection(mousePos);
            return;
        }

        _currentHoldTime += Time.deltaTime;
        progressGauge.fillAmount = _currentHoldTime / requiredHoldTime;

        if (targetPet != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(targetPet.position);
            progressGauge.transform.position = screenPos;
        }
        else
        {
            progressGauge.transform.position = mousePos;
        }

        if (_currentHoldTime >= requiredHoldTime)
        {
            OpenMenu(progressGauge.transform.position);
        }
    }

    void UpdateSelection(Vector3 mousePos)
    {
        float dist = Vector3.Distance(_menuCenterPos, mousePos);
        
        if (dist > selectionDeadzone)
        {
            Vector3 dir = mousePos - _menuCenterPos;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            // 360도를 3등분 (각 120도)
            // 0: 상단 (30 ~ 150도) - 밥주기
            if (angle >= 30f && angle < 150f)
            {
                _selectedItemIndex = 0;
            }
            // 1: 좌하단 (150 ~ 270도) - 레벨 초기화
            else if (angle >= 150f && angle < 270f)
            {
                _selectedItemIndex = 2;
            }
            // 2: 우하단 (270 ~ 30도) - 즉시 밥주기
            else
            {
                _selectedItemIndex = 1;
            }
            
            HighlightButtons();
        }
        else
        {
            _selectedItemIndex = -1;
            HighlightButtons();
        }
    }

    void HighlightButtons()
    {
        if (feedButton != null)
            feedButton.transform.localScale = (_selectedItemIndex == 0) ? Vector3.one * 1.3f : Vector3.one;
        
        if (instantFeedButton != null)
            instantFeedButton.transform.localScale = (_selectedItemIndex == 1) ? Vector3.one * 1.3f : Vector3.one;

        if (resetButton != null)
            resetButton.transform.localScale = (_selectedItemIndex == 2) ? Vector3.one * 1.3f : Vector3.one;
    }

    public void CancelLongPress()
    {
        if (_isMenuOpen)
        {
            SubmitSelection();
            return;
        }
        
        _currentHoldTime = 0f;
        progressGauge.fillAmount = 0;
        progressGauge.gameObject.SetActive(false);
    }

    void SubmitSelection()
    {
        switch (_selectedItemIndex)
        {
            case 0: OnClickFeed(); break;
            case 1: OnClickInstantFeed(); break;
            case 2: OnClickReset(); break;
            default: CloseMenu(); break;
        }
    }

    void OpenMenu(Vector3 position)
    {
        _isMenuOpen = true;
        _menuCenterPos = position;
        _selectedItemIndex = -1;
        
        menuRoot.SetActive(true);
        menuRoot.transform.position = position;
        progressGauge.gameObject.SetActive(false);
        HighlightButtons();
    }

    public void CloseMenu()
    {
        _isMenuOpen = false;
        menuRoot.SetActive(false);
        _currentHoldTime = 0f;
    }

    public void OnClickFeed()
    {
        if (PetGrowthController.Instance != null)
            PetGrowthController.Instance.TryFeed(false);
        CloseMenu();
    }

    public void OnClickInstantFeed()
    {
        if (PetGrowthController.Instance != null)
            PetGrowthController.Instance.TryFeed(true);
        CloseMenu();
    }

    public void OnClickReset()
    {
        if (PetGrowthController.Instance != null)
            PetGrowthController.Instance.ResetLevel();
        CloseMenu();
    }
}
