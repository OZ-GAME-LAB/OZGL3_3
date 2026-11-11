using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 플레이어가 죽으면 뜨는 창. 부활 버튼을 누르면 화면이 어두워진 사이에 데이터의 부활 위치로 옮기고 되살린다.
public class DeathUI : UIBase
{
    private const string PrefabPath = "UI/MainUI/DeathUI";
    private const float ReviveWaitLimit = 1.5f;     // 페이드 신호가 오지 않을 때 기다리는 최대 시간

    private static DeathUI _instance;

    [SerializeField] private TMP_FontAsset _font;
    [SerializeField] private float _fadeInDuration = 0.8f;

    private CanvasGroup _canvasGroup;
    private Button _buttonRevive;
    private float _elapsed;
    private float _reviveWait;
    private bool _isReviving;
    private Vector3? _respawnPosition;
    private MapRoot _respawnMap;

    public static bool IsOpen
    {
        get { return _instance != null && _instance.gameObject.activeSelf == true; }
    }

    public static void Open()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        if (_instance == null)
        {
            // MainHUD가 올라가 있는 캔버스 위에 만든다.
            UIBase hud = UIManager.Instance.GetOpenedUI(UIRootType.MainUI, UIType.MainHUD);
            GameObject prefab = Resources.Load<GameObject>(PrefabPath);

            if (hud == null || prefab == null)
            {
                Logger.LogError($"DeathUI: MainHUD 또는 Resources/{PrefabPath} 프리팹을 찾지 못했습니다.");
                return;
            }

            GameObject created = Instantiate(prefab, hud.transform.parent);
            _instance = created.GetComponent<DeathUI>();

            if (_instance == null)
            {
                Logger.LogError("DeathUI: 프리팹에 DeathUI 컴포넌트가 없습니다.");
                Destroy(created);
                return;
            }
        }

        _instance.transform.SetAsLastSibling();
        _instance.gameObject.SetActive(true);
    }

    public static void Close()
    {
        if (_instance != null)
        {
            _instance.gameObject.SetActive(false);
        }
    }

    private void Awake()
    {
        _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        TextMeshProUGUI title = CreateText("Text_Title", transform, 72f, TextAlignmentOptions.Center);
        title.text = "쓰러졌습니다";
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 70f);
        titleRect.sizeDelta = new Vector2(900f, 110f);

        RectTransform buttonRect = CreateRect("Button_Revive", transform);
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, -70f);
        buttonRect.sizeDelta = new Vector2(300f, 76f);

        Image buttonImage = buttonRect.gameObject.AddComponent<Image>();
        buttonImage.color = new Color(0.14f, 0.19f, 0.27f, 1f);

        _buttonRevive = buttonRect.gameObject.AddComponent<Button>();
        _buttonRevive.targetGraphic = buttonImage;

        ColorBlock colors = _buttonRevive.colors;
        colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.6f);
        _buttonRevive.colors = colors;

        TextMeshProUGUI label = CreateText("Text_Label", buttonRect, 34f, TextAlignmentOptions.Center);
        label.text = "부활";
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;
    }

    private void OnEnable()
    {
        _elapsed = 0f;
        _reviveWait = 0f;
        _isReviving = false;
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _buttonRevive.interactable = true;
        _buttonRevive.onClick.AddListener(OnClickRevive);
    }

    private void OnDisable()
    {
        _buttonRevive.onClick.RemoveListener(OnClickRevive);
        FadeUI.OnFadeBlack -= HandleFadeBlack;
    }

    private void Update()
    {
        if (_isReviving == true)
        {
            // 페이드 신호가 오지 않아도 플레이어가 멈춰 있지 않게 한다.
            _reviveWait += Time.unscaledDeltaTime;
            if (_reviveWait >= ReviveWaitLimit)
            {
                HandleFadeBlack();
            }

            return;
        }

        _elapsed += Time.unscaledDeltaTime;
        float alpha = Mathf.Clamp01(_elapsed / _fadeInDuration);
        _canvasGroup.alpha = alpha;
        _canvasGroup.blocksRaycasts = alpha >= 0.99f;
    }

    private void OnClickRevive()
    {
        if (_isReviving == true)
        {
            return;
        }

        _isReviving = true;
        _reviveWait = 0f;
        _buttonRevive.interactable = false;

        PlayerRespawn.Resolve(out _respawnPosition, out _respawnMap);

        // 화면이 완전히 어두워진 순간에 옮긴다.
        FadeUI.OnFadeBlack += HandleFadeBlack;
        UIManager.Instance.OpenFadeUI();
    }

    private void HandleFadeBlack()
    {
        FadeUI.OnFadeBlack -= HandleFadeBlack;
        _isReviving = false;

        PlayerRespawn.Execute(_respawnPosition, _respawnMap);
        gameObject.SetActive(false);
    }

    private static RectTransform CreateRect(string objectName, Transform parent)
    {
        GameObject created = new GameObject(objectName, typeof(RectTransform));
        RectTransform rect = created.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private TextMeshProUGUI CreateText(string objectName, Transform parent, float size, TextAlignmentOptions alignment)
    {
        RectTransform rect = CreateRect(objectName, parent);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();

        if (_font != null)
        {
            text.font = _font;
        }

        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }
}
