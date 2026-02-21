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
    public GameObject quitButton; // 앱 종료 아이콘/버튼

    [Header("Settings")]
    public float requiredHoldTime = 1.0f;
    public float selectionDeadzone = 50f; // 중심에서 이 거리 이상 나가야 선택됨
    
    [Header("UI Glass Effects")]
    public UnityEngine.UI.Image menuBackground;
    private Material _glassMaterial;
    private static readonly int GrainAmount = Shader.PropertyToID("_GrainAmount");
    private static readonly int EdgeHighlight = Shader.PropertyToID("_EdgeHighlight");

    private float _currentHoldTime = 0f;
    private bool _isMenuOpen = false;
    private Vector3 _menuCenterPos;
    private int _selectedItemIndex = -1; // -1: 없음, 0: 밥주기, 1: 즉시밥주기, 2: 레벨초기화

    void Start()
    {
        if (menuBackground != null)
        {
            // 인스턴스화하여 개별 제어 가능하게 함
            _glassMaterial = new Material(menuBackground.material);
            menuBackground.material = _glassMaterial;
        }
    }

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

            // 360도를 4등분 (각 90도)
            // 0: 상단 (45 ~ 135도) - 밥주기
            if (angle >= 45f && angle < 135f)
            {
                _selectedItemIndex = 0;
            }
            // 1: 좌측 (135 ~ 225도) - 레벨 초기화
            else if (angle >= 135f && angle < 225f)
            {
                _selectedItemIndex = 2;
            }
            // 2: 하단 (225 ~ 315도) - 앱 종료
            else if (angle >= 225f && angle < 315f)
            {
                _selectedItemIndex = 3;
            }
            // 3: 우측 (315 ~ 45도) - 즉시 밥주기
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

        if (quitButton != null)
            quitButton.transform.localScale = (_selectedItemIndex == 3) ? Vector3.one * 1.3f : Vector3.one;
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
            case 3: OnClickQuit(); break;
            default: CloseMenu(); break;
        }
    }

    public void OnClickQuit()
    {
        Debug.Log("[System] Application Quit requested via Radial Menu.");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private Coroutine _menuAnimation;

    public void CloseMenu()
    {
        if (!_isMenuOpen) return;
        
        _currentHoldTime = 0f; // 홀드 시간 초기화
        if (_menuAnimation != null) StopCoroutine(_menuAnimation);
        _menuAnimation = StartCoroutine(AnimateMenu(false));
    }

    private void OpenMenu(Vector3 position)
    {
        _isMenuOpen = true;
        _menuCenterPos = position;
        _selectedItemIndex = -1;
        
        menuRoot.SetActive(true);
        menuRoot.transform.position = position;
        progressGauge.gameObject.SetActive(false);
        
        if (_menuAnimation != null) StopCoroutine(_menuAnimation);
        _menuAnimation = StartCoroutine(AnimateMenu(true));
        
        HighlightButtons();
    }

    private IEnumerator AnimateMenu(bool open)
    {
        float duration = 0.25f;
        float elapsed = 0f;
        
        Vector3 startScale = open ? Vector3.zero : Vector3.one;
        Vector3 endScale = open ? Vector3.one : Vector3.zero;
        
        CanvasGroup cg = menuRoot.GetComponent<CanvasGroup>();
        if (cg == null) cg = menuRoot.AddComponent<CanvasGroup>();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (open)
            {
                // Overshoot (Back Out) Easing: 1.1배까지 커졌다가 1.0으로 안착
                float overshootT = Mathf.Sin(t * Mathf.PI * 0.5f); // 기본 Sine Ease Out
                float scale = open ? 
                    (t < 0.7f ? Mathf.Lerp(0, 1.15f, t / 0.7f) : Mathf.Lerp(1.15f, 1f, (t - 0.7f) / 0.3f)) : 
                    Mathf.Lerp(1, 0, t);
                
                menuRoot.transform.localScale = Vector3.one * scale;
                cg.alpha = t;

                if (_glassMaterial != null)
                {
                    _glassMaterial.SetFloat(GrainAmount, Mathf.Lerp(0, 0.05f, t));
                    _glassMaterial.SetFloat(EdgeHighlight, Mathf.Lerp(0, 0.8f, t));
                }
            }
            else
            {
                menuRoot.transform.localScale = Vector3.Lerp(startScale, endScale, t);
                cg.alpha = 1 - t;

                if (_glassMaterial != null)
                {
                    _glassMaterial.SetFloat(GrainAmount, Mathf.Lerp(0.05f, 0, t));
                    _glassMaterial.SetFloat(EdgeHighlight, Mathf.Lerp(0.8f, 0, t));
                }
            }
            
            yield return null;
        }

        menuRoot.transform.localScale = endScale;
        cg.alpha = open ? 1f : 0f;
        
        if (!open)
        {
            menuRoot.SetActive(false);
            _isMenuOpen = false;
        }
    }

    public void OnClickFeed()
    {
        if (targetPet != null)
        {
            var controller = targetPet.GetComponent<PetGrowthController>();
            if (controller != null) controller.TryFeed(false);
        }
        CloseMenu();
    }

    public void OnClickInstantFeed()
    {
        if (targetPet != null)
        {
            var controller = targetPet.GetComponent<PetGrowthController>();
            if (controller != null) controller.TryFeed(true);
        }
        CloseMenu();
    }

    public void OnClickReset()
    {
        if (targetPet != null)
        {
            var controller = targetPet.GetComponent<PetGrowthController>();
            if (controller != null) controller.ResetLevel();
        }
        CloseMenu();
    }
}
