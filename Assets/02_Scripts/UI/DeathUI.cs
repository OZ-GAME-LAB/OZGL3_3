using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 플레이어가 죽으면 뜨는 창. 화면이 어두워지고 빛 알갱이가 떠오른 뒤, 제목과 부활할 지역이 차례로 나타난다.
// 부활 버튼을 누르면 화면이 어두워진 사이에 데이터의 부활 위치로 옮기고 되살린다.
public class DeathUI : UIBase
{
    private const string PrefabPath = "UI/MainUI/DeathUI";
    private const float ReviveWaitLimit = 1.5f;     // 페이드 신호가 오지 않을 때 기다리는 최대 시간
    private const int MoteCount = 36;

    private static readonly Color GlowColor = new Color(0.66f, 0.76f, 0.9f, 1f);
    private static readonly Color BloodColor = new Color(0.64f, 0.2f, 0.24f, 1f);
    private static readonly Color GoldColor = new Color(0.85f, 0.76f, 0.54f, 1f);
    private static readonly Color MuteColor = new Color(0.55f, 0.59f, 0.65f, 1f);
    private static readonly Color BaseTextColor = new Color(0.87f, 0.9f, 0.93f, 1f);

    private static DeathUI _instance;
    private static Sprite _softCircleSprite;
    private static Sprite _vignetteSprite;

    private class Mote
    {
        public RectTransform Rect;
        public Image Image;
        public float Delay;
        public float Life;
        public float StartX;
        public float Drift;
        public float Speed;
        public float Size;
        public float Phase;
    }

    [SerializeField] private TMP_FontAsset _font;
    [SerializeField] private float _fadeInDuration = 0.8f;       // 사용하지 않음(이전 프리팹 값 보존용)

    [Header("시간(초)")]
    [SerializeField] private float _dimEnd = 1.4f;
    [SerializeField] private float _titleStart = 1.1f;
    [SerializeField] private float _buttonStart = 3.0f;

    private readonly Mote[] _motes = new Mote[MoteCount];

    private Image _imageDim;
    private Image _imageVignette;
    private RectTransform _barTop;
    private RectTransform _barBottom;
    private TextMeshProUGUI _textEyebrow;
    private TextMeshProUGUI _textTitle;
    private RectTransform _rectOrnament;
    private Image _imageDiamond;
    private Image _imageOrnamentLine;
    private TextMeshProUGUI _textWhere;
    private TextMeshProUGUI _textHint;
    private CanvasGroup _groupButton;
    private Button _buttonRevive;

    private float _dimBaseAlpha;
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
        _imageDim = GetComponent<Image>();
        _dimBaseAlpha = _imageDim != null ? _imageDim.color.a : 0.8f;

        BuildVignette();
        BuildMotes();
        BuildBars();
        BuildTexts();
        BuildButton();
    }

    private void OnEnable()
    {
        _elapsed = 0f;
        _reviveWait = 0f;
        _isReviving = false;

        SetRegionTexts();
        ApplyTimeline(0f);

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
        ApplyTimeline(_elapsed);

        bool canRevive = _elapsed >= _buttonStart + 0.2f;
        if (canRevive == true
            && (Input.GetKeyDown(KeyCode.Return) == true || Input.GetKeyDown(KeyCode.KeypadEnter) == true))
        {
            OnClickRevive();
        }
    }

    #region 연출

    private void ApplyTimeline(float time)
    {
        // 화면이 어두워지고 붉은 비네트가 번진다.
        SetAlpha(_imageDim, _dimBaseAlpha * EaseOut(Progress(time, 0f, _dimEnd)));
        SetAlpha(_imageVignette, EaseOut(Progress(time, 0.2f, 2f)));

        // 위아래 검은 띠
        float barScale = EaseOut(Progress(time, 0.1f, 1.2f));
        _barTop.localScale = new Vector3(1f, barScale, 1f);
        _barBottom.localScale = new Vector3(1f, barScale, 1f);

        // 지역 이름 → 제목
        SetAlpha(_textEyebrow, EaseOut(Progress(time, _titleStart - 0.1f, _titleStart + 0.9f)));

        float titleProgress = EaseOut(Progress(time, _titleStart, _titleStart + 1.6f));
        SetAlpha(_textTitle, titleProgress);
        _textTitle.characterSpacing = Mathf.Lerp(60f, 30f, titleProgress);
        _textTitle.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.06f, 1f, titleProgress);

        // 장식선과 마름모
        float ornamentWidth = Mathf.Lerp(0f, 380f, EaseOut(Progress(time, _titleStart + 0.8f, _titleStart + 1.9f)));
        _rectOrnament.sizeDelta = new Vector2(ornamentWidth, 2f);
        SetAlpha(_imageOrnamentLine, 0.85f * Progress(time, _titleStart + 0.8f, _titleStart + 1.2f));
        SetAlpha(_imageDiamond, Progress(time, _titleStart + 1.1f, _titleStart + 1.7f));

        // 부활할 지역, 버튼, 안내
        SetAlpha(_textWhere, Progress(time, _titleStart + 1.4f, _titleStart + 2.2f));
        _groupButton.alpha = EaseOut(Progress(time, _buttonStart, _buttonStart + 0.8f));
        bool isButtonReady = time >= _buttonStart + 0.2f;
        _groupButton.interactable = isButtonReady;
        _groupButton.blocksRaycasts = isButtonReady;
        SetAlpha(_textHint, Progress(time, _buttonStart + 0.4f, _buttonStart + 1.2f));

        UpdateMotes(time);
    }

    private void UpdateMotes(float time)
    {
        for (int i = 0; i < _motes.Length; i++)
        {
            Mote mote = _motes[i];
            float age = time - 0.2f - mote.Delay;
            if (age <= 0f || age >= mote.Life)
            {
                SetAlpha(mote.Image, 0f);
                continue;
            }

            float k = age / mote.Life;
            float x = mote.StartX + mote.Drift * k + Mathf.Sin(age * 2f + mote.Phase) * 10f;
            float y = -50f + mote.Speed * age;

            mote.Rect.anchoredPosition = new Vector2(x, y);
            SetAlpha(mote.Image, Mathf.Sin(Mathf.PI * k) * 0.9f);
        }
    }

    private void SetRegionTexts()
    {
        string currentName = string.Empty;
        string respawnName = string.Empty;

        if (GameManager.Instance != null)
        {
            string regionId = GameManager.Area.CurrentRegionId;
            if (string.IsNullOrEmpty(regionId) == false
                && GameManager.DataTable.TryGetAreaSprite(regionId, out AreaTextData current) == true)
            {
                currentName = current.MapName;

                if (string.IsNullOrEmpty(current.RespawnMap) == false
                    && GameManager.DataTable.TryGetAreaSprite(current.RespawnMap, out AreaTextData respawn) == true)
                {
                    respawnName = respawn.MapName;
                }
            }
        }

        _textEyebrow.text = currentName;

        if (string.IsNullOrEmpty(respawnName) == true)
        {
            _textWhere.text = string.Empty;
        }
        else
        {
            _textWhere.text = $"<color=#DFE6EE>{respawnName}</color>에서 다시 일어납니다";
        }
    }

    private static float Progress(float time, float start, float end)
    {
        return Mathf.Clamp01((time - start) / (end - start));
    }

    private static float EaseOut(float value)
    {
        float inverse = 1f - value;
        return 1f - inverse * inverse * inverse;
    }

    private static void SetAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    #endregion

    #region 부활

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
        _buttonRevive.interactable = true;

        PlayerRespawn.Execute(_respawnPosition, _respawnMap);
        gameObject.SetActive(false);
    }

    #endregion

    #region 구성

    private void BuildVignette()
    {
        RectTransform rect = CreateRect("Vignette", transform);
        Stretch(rect);
        _imageVignette = rect.gameObject.AddComponent<Image>();
        _imageVignette.sprite = GetVignetteSprite();
        _imageVignette.raycastTarget = false;
        _imageVignette.color = new Color(1f, 1f, 1f, 0f);
    }

    private void BuildMotes()
    {
        RectTransform root = CreateRect("Motes", transform);
        Stretch(root);

        for (int i = 0; i < _motes.Length; i++)
        {
            Mote mote = new Mote();
            mote.Rect = CreateRect("Mote", root);
            mote.Rect.anchorMin = new Vector2(0.5f, 0.5f);
            mote.Rect.anchorMax = new Vector2(0.5f, 0.5f);
            mote.Rect.pivot = new Vector2(0.5f, 0.5f);

            mote.Size = Random.Range(10f, 26f);
            mote.Rect.sizeDelta = new Vector2(mote.Size, mote.Size);

            mote.Image = mote.Rect.gameObject.AddComponent<Image>();
            mote.Image.sprite = GetSoftCircleSprite();
            mote.Image.raycastTarget = false;
            mote.Image.color = new Color(GlowColor.r, GlowColor.g, GlowColor.b, 0f);

            mote.Delay = Random.Range(0f, 1.2f);
            mote.Life = Random.Range(2.4f, 4.2f);
            mote.StartX = Random.Range(-30f, 30f);
            mote.Drift = Random.Range(-60f, 60f);
            mote.Speed = Random.Range(60f, 150f);
            mote.Phase = Random.Range(0f, 6.28f);

            _motes[i] = mote;
        }
    }

    private void BuildBars()
    {
        _barTop = CreateBar("Bar_Top", true);
        _barBottom = CreateBar("Bar_Bottom", false);
    }

    private RectTransform CreateBar(string objectName, bool isTop)
    {
        RectTransform rect = CreateRect(objectName, transform);
        rect.anchorMin = new Vector2(0f, isTop ? 0.91f : 0f);
        rect.anchorMax = new Vector2(1f, isTop ? 1f : 0.09f);
        rect.pivot = new Vector2(0.5f, isTop ? 1f : 0f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;
        return rect;
    }

    private void BuildTexts()
    {
        _textEyebrow = CreateText("Text_Eyebrow", transform, 24f, TextAlignmentOptions.Center, GoldColor);
        _textEyebrow.characterSpacing = 25f;
        PlaceCentered(_textEyebrow.rectTransform, new Vector2(0f, 150f), new Vector2(900f, 40f));

        _textTitle = CreateText("Text_Title", transform, 72f, TextAlignmentOptions.Center, BaseTextColor);
        _textTitle.text = "쓰러졌습니다";
        PlaceCentered(_textTitle.rectTransform, new Vector2(0f, 70f), new Vector2(1000f, 110f));

        _rectOrnament = CreateRect("Ornament", transform);
        PlaceCentered(_rectOrnament, new Vector2(0f, 4f), new Vector2(0f, 2f));
        _imageOrnamentLine = _rectOrnament.gameObject.AddComponent<Image>();
        _imageOrnamentLine.color = new Color(GlowColor.r, GlowColor.g, GlowColor.b, 0f);
        _imageOrnamentLine.raycastTarget = false;

        RectTransform diamond = CreateRect("Diamond", transform);
        PlaceCentered(diamond, new Vector2(0f, 4f), new Vector2(14f, 14f));
        diamond.localRotation = Quaternion.Euler(0f, 0f, 45f);
        _imageDiamond = diamond.gameObject.AddComponent<Image>();
        _imageDiamond.color = new Color(BloodColor.r, BloodColor.g, BloodColor.b, 0f);
        _imageDiamond.raycastTarget = false;

        _textWhere = CreateText("Text_Where", transform, 26f, TextAlignmentOptions.Center, MuteColor);
        _textWhere.richText = true;
        PlaceCentered(_textWhere.rectTransform, new Vector2(0f, -36f), new Vector2(1000f, 44f));

        _textHint = CreateText("Text_Hint", transform, 18f, TextAlignmentOptions.Center, new Color(0.4f, 0.45f, 0.5f, 1f));
        _textHint.text = "ENTER";
        _textHint.characterSpacing = 20f;
        PlaceCentered(_textHint.rectTransform, new Vector2(0f, -210f), new Vector2(400f, 30f));
    }

    private void BuildButton()
    {
        RectTransform buttonRect = CreateRect("Button_Revive", transform);
        PlaceCentered(buttonRect, new Vector2(0f, -120f), new Vector2(300f, 76f));

        Image buttonImage = buttonRect.gameObject.AddComponent<Image>();
        buttonImage.color = new Color(0.05f, 0.08f, 0.12f, 0.8f);

        _groupButton = buttonRect.gameObject.AddComponent<CanvasGroup>();

        _buttonRevive = buttonRect.gameObject.AddComponent<Button>();
        _buttonRevive.targetGraphic = buttonImage;

        ColorBlock colors = _buttonRevive.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.6f, 1.7f, 1.9f, 1f);
        colors.pressedColor = new Color(0.7f, 0.75f, 0.8f, 1f);
        colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.6f);
        _buttonRevive.colors = colors;

        // 네 귀퉁이만 선을 그린 틀
        BuildCorner(buttonRect, new Vector2(0f, 1f));
        BuildCorner(buttonRect, new Vector2(1f, 1f));
        BuildCorner(buttonRect, new Vector2(0f, 0f));
        BuildCorner(buttonRect, new Vector2(1f, 0f));

        TextMeshProUGUI label = CreateText("Text_Label", buttonRect, 34f, TextAlignmentOptions.Center, BaseTextColor);
        label.text = "부활";
        label.characterSpacing = 30f;
        Stretch(label.rectTransform);
    }

    private void BuildCorner(RectTransform parent, Vector2 corner)
    {
        const float Length = 14f;
        const float Thickness = 2f;

        CreateCornerLine(parent, corner, new Vector2(Length, Thickness));
        CreateCornerLine(parent, corner, new Vector2(Thickness, Length));
    }

    private void CreateCornerLine(RectTransform parent, Vector2 corner, Vector2 size)
    {
        RectTransform rect = CreateRect("Corner", parent);
        rect.anchorMin = corner;
        rect.anchorMax = corner;
        rect.pivot = corner;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = GlowColor;
        image.raycastTarget = false;
    }

    private static void PlaceCentered(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static RectTransform CreateRect(string objectName, Transform parent)
    {
        GameObject created = new GameObject(objectName, typeof(RectTransform));
        RectTransform rect = created.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private TextMeshProUGUI CreateText(string objectName, Transform parent, float size, TextAlignmentOptions alignment, Color color)
    {
        RectTransform rect = CreateRect(objectName, parent);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();

        if (_font != null)
        {
            text.font = _font;
        }

        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    // 가운데는 비어 있고 가장자리로 갈수록 붉게 짙어지는 비네트
    private static Sprite GetVignetteSprite()
    {
        if (_vignetteSprite != null)
        {
            return _vignetteSprite;
        }

        const int Size = 128;
        Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float dx = (x + 0.5f) / Size * 2f - 1f;
                float dy = (y + 0.5f) / Size * 2f - 1f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 1.35f, distance)) * 0.5f;
                texture.SetPixel(x, y, new Color(0.5f, 0.08f, 0.12f, alpha));
            }
        }

        texture.Apply();
        _vignetteSprite = Sprite.Create(texture, new Rect(0f, 0f, Size, Size), new Vector2(0.5f, 0.5f), 100f);
        return _vignetteSprite;
    }

    // 가장자리가 부드럽게 사라지는 빛 알갱이
    private static Sprite GetSoftCircleSprite()
    {
        if (_softCircleSprite != null)
        {
            return _softCircleSprite;
        }

        const int Size = 64;
        Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float dx = (x + 0.5f) / Size * 2f - 1f;
                float dy = (y + 0.5f) / Size * 2f - 1f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Pow(Mathf.Clamp01(1f - distance), 2f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        _softCircleSprite = Sprite.Create(texture, new Rect(0f, 0f, Size, Size), new Vector2(0.5f, 0.5f), 100f);
        return _softCircleSprite;
    }

    #endregion
}
