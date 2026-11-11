using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 월드맵 화면. 가 본 방만 채워서 보여 주고, 드래그로 이동하고 휠로 확대한다.
// 방 위치와 크기는 각 맵의 AreaZone 콜라이더에서 읽어 오므로 별도 좌표 표가 필요 없다.
public class WorldMapUI : UIBase
{
    private const string PrefabPath = "UI/MainUI/WorldMapUI";
    private const float BorderThickness = 3f;
    private const float PanKeySpeed = 600f;

    private static WorldMapUI _instance;

    [SerializeField] private TMP_FontAsset _font;
    [SerializeField] private float _unitScale = 6f;     // 월드 1칸이 화면에서 차지하는 크기(px)
    [SerializeField] private float _minZoom = 0.3f;
    [SerializeField] private float _maxZoom = 3f;

    private class RoomView
    {
        public string Key;
        public Rect WorldRect;
        public Color Color;
        public RectTransform Root;
        public UnityEngine.UI.Image Border;
        public UnityEngine.UI.Image Fill;
        public TextMeshProUGUI Label;
    }

    private readonly List<RoomView> _rooms = new List<RoomView>();

    private Canvas _canvas;
    private RectTransform _content;
    private RectTransform _pin;
    private Vector2 _focus;
    private float _zoom = 1f;
    private float _prevTimeScale = 1f;
    private bool _isDragging;
    private Vector2 _lastMouse;

    public static bool IsOpen
    {
        get { return _instance != null && _instance.gameObject.activeSelf == true; }
    }

    public static void Toggle()
    {
        if (IsOpen == true)
        {
            Close();
        }
        else
        {
            Open();
        }
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
                Logger.LogError($"WorldMapUI: MainHUD 또는 Resources/{PrefabPath} 프리팹을 찾지 못했습니다.");
                return;
            }

            GameObject created = Instantiate(prefab, hud.transform.parent);
            _instance = created.GetComponent<WorldMapUI>();

            if (_instance == null)
            {
                Logger.LogError("WorldMapUI: 프리팹에 WorldMapUI 컴포넌트가 없습니다.");
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
        _canvas = GetComponentInParent<Canvas>();

        _content = CreateRect("Content", transform);
        _content.anchorMin = new Vector2(0.5f, 0.5f);
        _content.anchorMax = new Vector2(0.5f, 0.5f);
        _content.pivot = new Vector2(0.5f, 0.5f);
        _content.sizeDelta = Vector2.zero;

        RectTransform pinRect = CreateRect("Pin", _content);
        pinRect.sizeDelta = new Vector2(14f, 14f);
        pinRect.localRotation = Quaternion.Euler(0f, 0f, 45f);
        UnityEngine.UI.Image pinImage = pinRect.gameObject.AddComponent<UnityEngine.UI.Image>();
        pinImage.color = Color.white;
        pinImage.raycastTarget = false;
        _pin = pinRect;

        TextMeshProUGUI title = CreateText("Text_Title", transform, 36f, TextAlignmentOptions.TopLeft);
        title.text = "지도";
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(0f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = new Vector2(40f, -30f);
        titleRect.sizeDelta = new Vector2(400f, 60f);

        TextMeshProUGUI hint = CreateText("Text_Hint", transform, 20f, TextAlignmentOptions.Bottom);
        hint.text = "드래그: 이동     휠: 확대/축소     Space: 현재 위치     M: 닫기";
        hint.color = new Color(0.62f, 0.68f, 0.75f, 1f);
        RectTransform hintRect = hint.rectTransform;
        hintRect.anchorMin = new Vector2(0.5f, 0f);
        hintRect.anchorMax = new Vector2(0.5f, 0f);
        hintRect.pivot = new Vector2(0.5f, 0f);
        hintRect.anchoredPosition = new Vector2(0f, 24f);
        hintRect.sizeDelta = new Vector2(1000f, 40f);
    }

    private void OnEnable()
    {
        // 지도를 보는 동안 게임을 멈춘다.
        _prevTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;

        WorldMapData.OnChanged += Refresh;

        BuildRooms();
        _zoom = 1f;
        FocusOnPlayer();
        Refresh();
        ApplyView();
    }

    private void OnDisable()
    {
        WorldMapData.OnChanged -= Refresh;
        Time.timeScale = _prevTimeScale;
        _isDragging = false;
    }

    private void Update()
    {
        HandleInput();
        ApplyView();
    }

    private void HandleInput()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            _zoom = Mathf.Clamp(_zoom * Mathf.Pow(1.12f, scroll), _minZoom, _maxZoom);
        }

        float scaleFactor = _canvas != null ? _canvas.rootCanvas.scaleFactor : 1f;
        float worldPerCanvasPixel = 1f / (_unitScale * _zoom);

        if (Input.GetMouseButtonDown(0) == true)
        {
            _isDragging = true;
            _lastMouse = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0) == true)
        {
            _isDragging = false;
        }

        if (_isDragging == true && Input.GetMouseButton(0) == true)
        {
            Vector2 currentMouse = Input.mousePosition;
            Vector2 delta = (currentMouse - _lastMouse) / scaleFactor;
            _lastMouse = currentMouse;
            _focus -= delta * worldPerCanvasPixel;
        }

        Vector2 direction = Vector2.zero;
        if (Input.GetKey(KeyCode.LeftArrow) == true) { direction.x -= 1f; }
        if (Input.GetKey(KeyCode.RightArrow) == true) { direction.x += 1f; }
        if (Input.GetKey(KeyCode.DownArrow) == true) { direction.y -= 1f; }
        if (Input.GetKey(KeyCode.UpArrow) == true) { direction.y += 1f; }
        _focus += direction * (PanKeySpeed * Time.unscaledDeltaTime * worldPerCanvasPixel);

        if (Input.GetKeyDown(KeyCode.Space) == true)
        {
            FocusOnPlayer();
        }
    }

    private void FocusOnPlayer()
    {
        if (WorldMapTracker.TryGetPlayerPosition(out Vector2 playerPosition) == true)
        {
            _focus = playerPosition;
        }
        else if (_rooms.Count > 0)
        {
            _focus = _rooms[0].WorldRect.center;
        }
        else
        {
            _focus = Vector2.zero;
        }
    }

    private void ApplyView()
    {
        _content.localScale = new Vector3(_zoom, _zoom, 1f);
        _content.anchoredPosition = -_focus * (_unitScale * _zoom);

        // 현재 위치 핀은 확대해도 같은 크기로, 살짝 깜빡이게 한다.
        if (WorldMapTracker.TryGetPlayerPosition(out Vector2 playerPosition) == true)
        {
            _pin.gameObject.SetActive(true);
            _pin.anchoredPosition = playerPosition * _unitScale;
            float pulse = 1f + 0.2f * Mathf.Sin(Time.unscaledTime * 5f);
            _pin.localScale = new Vector3(pulse / _zoom, pulse / _zoom, 1f);
        }
        else
        {
            _pin.gameObject.SetActive(false);
        }
    }

    private void BuildRooms()
    {
        for (int i = _content.childCount - 1; i >= 0; i--)
        {
            Transform child = _content.GetChild(i);
            if (child != _pin)
            {
                Destroy(child.gameObject);
            }
        }

        _rooms.Clear();

        AreaZone[] zones = FindObjectsByType<AreaZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < zones.Length; i++)
        {
            AreaZone zone = zones[i];
            if (WorldMapTracker.TryGetWorldRect(zone, out Rect worldRect) == false)
            {
                continue;
            }

            string key = WorldMapTracker.GetRoomKey(zone);
            RoomView view = new RoomView();
            view.Key = key;
            view.WorldRect = worldRect;
            view.Color = GetRegionColor(zone.RegionId);

            view.Root = CreateRect("Room_" + key, _content);
            view.Root.anchorMin = new Vector2(0.5f, 0.5f);
            view.Root.anchorMax = new Vector2(0.5f, 0.5f);
            view.Root.pivot = new Vector2(0.5f, 0.5f);
            view.Root.sizeDelta = worldRect.size * _unitScale;
            view.Root.anchoredPosition = worldRect.center * _unitScale;

            view.Border = view.Root.gameObject.AddComponent<UnityEngine.UI.Image>();
            view.Border.raycastTarget = false;

            RectTransform fillRect = CreateRect("Fill", view.Root);
            StretchWithInset(fillRect, BorderThickness);
            view.Fill = fillRect.gameObject.AddComponent<UnityEngine.UI.Image>();
            view.Fill.raycastTarget = false;

            view.Label = CreateText("Label", view.Root, 16f, TextAlignmentOptions.Center);
            StretchWithInset(view.Label.rectTransform, 6f);
            view.Label.enableAutoSizing = true;
            view.Label.fontSizeMin = 6f;
            view.Label.fontSizeMax = 16f;
            view.Label.text = GetRoomLabel(zone.RegionId, key);

            _rooms.Add(view);
        }

        // 핀이 항상 방 위에 보이게 한다.
        _pin.SetAsLastSibling();
    }

    private void Refresh()
    {
        for (int i = 0; i < _rooms.Count; i++)
        {
            RoomView view = _rooms[i];
            bool isVisited = WorldMapData.IsVisited(view.Key);
            bool isVisible = isVisited == true || WorldMapData.IsRevealAll == true;

            view.Root.gameObject.SetActive(isVisible);
            if (isVisible == false)
            {
                continue;
            }

            Color color = view.Color;
            if (isVisited == true)
            {
                bool isCurrent = view.Key == WorldMapTracker.CurrentRoomKey;
                view.Border.color = isCurrent == true ? Color.white : color;
                view.Fill.color = new Color(color.r * 0.45f, color.g * 0.45f, color.b * 0.45f, 0.9f);
                view.Label.gameObject.SetActive(true);
            }
            else
            {
                // 지도 아이템으로만 보이는, 아직 안 가 본 방
                view.Border.color = new Color(color.r, color.g, color.b, 0.35f);
                view.Fill.color = new Color(color.r, color.g, color.b, 0.08f);
                view.Label.gameObject.SetActive(false);
            }
        }
    }

    // 지역 표(AreaText)의 MapName을 쓰고, 없으면 방 키로 대신한다.
    private static string GetRoomLabel(string regionId, string key)
    {
        if (string.IsNullOrEmpty(regionId) == false
            && GameManager.Instance != null
            && GameManager.DataTable.TryGetAreaSprite(regionId, out AreaTextData data) == true
            && string.IsNullOrEmpty(data.MapName) == false)
        {
            return data.MapName;
        }

        int slash = key.LastIndexOf('/');
        string label = slash >= 0 ? key.Substring(slash + 1) : key;

        if (label.StartsWith("Zone_") == true)
        {
            label = label.Substring(5);
        }

        return label;
    }

    private static Color GetRegionColor(string regionId)
    {
        if (string.IsNullOrEmpty(regionId) == false)
        {
            if (regionId.Contains("Forest") == true || regionId.Contains("Astra") == true)
            {
                return new Color(0.37f, 0.68f, 0.48f, 1f);
            }

            if (regionId.Contains("Ruin") == true)
            {
                return new Color(0.83f, 0.64f, 0.34f, 1f);
            }

            if (regionId.Contains("Guardian") == true)
            {
                return new Color(0.66f, 0.55f, 0.88f, 1f);
            }

            if (regionId.Contains("Swamp") == true)
            {
                return new Color(0.31f, 0.70f, 0.72f, 1f);
            }

            if (regionId.Contains("Snow") == true)
            {
                return new Color(0.62f, 0.78f, 0.95f, 1f);
            }
        }

        return new Color(0.60f, 0.65f, 0.70f, 1f);
    }

    private static RectTransform CreateRect(string objectName, Transform parent)
    {
        GameObject created = new GameObject(objectName, typeof(RectTransform));
        RectTransform rect = created.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void StretchWithInset(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
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
