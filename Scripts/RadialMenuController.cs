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

    [Header("Settings")]
    public float requiredHoldTime = 1.0f;
    public float selectionDeadzone = 50f; // 중심에서 이 거리 이상 나가야 선택됨
    
    private float _currentHoldTime = 0f;
    private bool _isMenuOpen = false;
    private Vector3 _menuCenterPos;
    private int _selectedItemIndex = -1; // 현재 선택된 인덱스 (-1: 없음, 0: 밥주기)

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
            // 중심에서 마우스의 방향(각도) 계산
            Vector3 dir = mousePos - _menuCenterPos;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            // 현재는 상단(밥주기) 하나만 있으므로 60도~120도 사이면 선택으로 간주
            if (angle >= 60f && angle <= 120f)
            {
                _selectedItemIndex = 0;
                HighlightButton(true);
            }
            else
            {
                _selectedItemIndex = -1;
                HighlightButton(false);
            }
        }
        else
        {
            _selectedItemIndex = -1;
            HighlightButton(false);
        }
    }

    void HighlightButton(bool highlight)
    {
        if (feedButton != null)
        {
            // 간단하게 버튼의 크기나 색상을 조절하여 피드백 제공
            feedButton.transform.localScale = highlight ? Vector3.one * 1.2f : Vector3.one;
        }
    }

    public void CancelLongPress()
    {
        // 마우스를 뗄 때 호출됨
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
        if (_selectedItemIndex == 0)
        {
            OnClickFeed();
        }
        else
        {
            CloseMenu();
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
        
        Debug.Log("방사형 메뉴가 열렸습니다. 항목을 선택하려면 드래그하세요.");
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
