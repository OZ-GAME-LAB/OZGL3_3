using UnityEngine;
using UnityEngine.UI;

// 타이틀 인트로: 검은 화면에 물방울이 떨어지고, 닿은 자리에서 빛이 번지며 배경이 드러난다.
// 로고와 메뉴는 처음부터 보이는 상태 그대로 두고, 배경 이미지만 마스크 셰이더로 가린다.
public class GameStartIntro : MonoBehaviour
{
    [SerializeField] private Image Image_Background;
    [SerializeField] private Image Image_Glow;
    [SerializeField] private Image Image_Flash;
    [SerializeField] private Image Image_Drop;
    [SerializeField] private Shader _revealShader;
    [SerializeField] private CanvasGroup _logoGroup;
    [SerializeField] private CanvasGroup _menuGroup;

    [Header("시간(초)")]
    [SerializeField] private float _dropStartTime = 0.6f;
    [SerializeField] private float _impactTime = 1.5f;
    [SerializeField] private float _revealEndTime = 4.9f;
    [SerializeField] private float _uiShowTime = 1.5f;       // 로고와 메뉴가 나타나기 시작하는 시간
    [SerializeField] private float _uiFadeDuration = 0.8f;

    [Header("모양")]
    [SerializeField] private Vector2 _impactOffset = new Vector2(0f, -65f);   // 화면 중앙 기준 물방울이 닿는 위치
    [SerializeField] private float _squash = 0.7f;                            // 작을수록 가로로 납작하게 번진다

    private static readonly int CenterId = Shader.PropertyToID("_Center");
    private static readonly int RectInfoId = Shader.PropertyToID("_RectInfo");
    private static readonly int RadiusId = Shader.PropertyToID("_Radius");
    private static readonly int AspectId = Shader.PropertyToID("_Aspect");
    private static readonly int SquashId = Shader.PropertyToID("_Squash");
    private static readonly int BoostId = Shader.PropertyToID("_Boost");

    private static readonly Color GlowColor = new Color(0.78f, 0.9f, 1f, 1f);
    private static readonly Color FlashColor = new Color(0.92f, 0.97f, 1f, 1f);
    private static readonly Color DropColor = new Color(1f, 1f, 1f, 1f);

    // 연출 크기는 가로 1400 화면을 기준으로 잡고, 실제 화면 폭에 맞춰 늘린다.
    private const float BaseWidth = 1400f;

    private Material _material;
    private float _elapsed;
    private bool _playing;

    private void OnEnable()
    {
        Begin();
    }

    private void OnDisable()
    {
        Finish();
    }

    private void OnDestroy()
    {
        if (_material != null)
        {
            Destroy(_material);
            _material = null;
        }
    }

    private void Update()
    {
        if (_playing == false)
        {
            return;
        }

        // 로딩 직후 프레임이 길게 튀어도 연출이 건너뛰어지지 않게 한다.
        _elapsed += Mathf.Min(Time.unscaledDeltaTime, 0.1f);

        if (_elapsed >= _revealEndTime + 0.6f)
        {
            Finish();
            return;
        }

        ApplyFrame();
    }

    private void Begin()
    {
        if (_revealShader == null || Image_Background == null)
        {
            Debug.LogWarning("[GameStartIntro] 셰이더나 배경 이미지가 연결되지 않아 인트로를 건너뜁니다.");
            return;
        }

        if (_material == null)
        {
            _material = new Material(_revealShader);
        }

        _elapsed = 0f;
        _playing = true;

        Image_Background.material = _material;
        Debug.Log("[GameStartIntro] 인트로 시작");
        ApplyFrame();
    }

    private void Finish()
    {
        _playing = false;

        // 기본 머티리얼로 되돌리면 배경이 온전히 보인다.
        if (Image_Background != null)
        {
            Image_Background.material = null;
        }

        SetVisible(Image_Glow, false);
        SetVisible(Image_Flash, false);
        SetVisible(Image_Drop, false);
        SetGroup(_logoGroup, 1f);
        SetGroup(_menuGroup, 1f);
    }

    private void ApplyFrame()
    {
        // 배경 이미지의 사각형을 기준으로 계산한다. 셰이더도 같은 사각형 안의 위치를 쓴다.
        Rect rect = Image_Background.rectTransform.rect;
        float width = Mathf.Max(rect.width, 1f);
        float height = Mathf.Max(rect.height, 1f);
        float aspect = width / height;
        float scale = width / BaseWidth;

        Vector2 centerUv = new Vector2(0.5f + _impactOffset.x / width, 0.5f + _impactOffset.y / height);

        // 가장 먼 모서리까지 닿는 반지름 (셰이더와 같은 단위)
        float farX = Mathf.Max(centerUv.x, 1f - centerUv.x) * aspect;
        float farY = Mathf.Max(centerUv.y, 1f - centerUv.y) / _squash;
        float maxRadius = Mathf.Sqrt(farX * farX + farY * farY) * 1.2f;

        float revealProgress = Mathf.InverseLerp(_impactTime, _revealEndTime, _elapsed);
        float radius = EaseOutCubic(revealProgress) * maxRadius;
        float boost = SmoothStep(Mathf.InverseLerp(_revealEndTime - 0.6f, _revealEndTime + 0.4f, _elapsed));

        _material.SetVector(CenterId, new Vector4(centerUv.x, centerUv.y, 0f, 0f));
        _material.SetVector(RectInfoId, new Vector4(rect.xMin, rect.yMin, width, height));
        _material.SetFloat(RadiusId, radius);
        _material.SetFloat(AspectId, aspect);
        _material.SetFloat(SquashId, _squash);
        _material.SetFloat(BoostId, boost);

        float uiAlpha = SmoothStep(Mathf.InverseLerp(_uiShowTime, _uiShowTime + _uiFadeDuration, _elapsed));
        SetGroup(_logoGroup, uiAlpha);
        SetGroup(_menuGroup, uiAlpha);

        UpdateGlow(scale);
        UpdateFlash(scale);
        UpdateDrop(scale, height);
    }

    // 닿은 자리에서 은은하게 퍼졌다가 사라지는 빛
    private void UpdateGlow(float scale)
    {
        float p = Mathf.InverseLerp(_impactTime, _revealEndTime + 0.6f, _elapsed);
        bool visible = p > 0f && p < 1f;
        SetVisible(Image_Glow, visible);

        if (visible == false)
        {
            return;
        }

        float radius = (60f + 380f * EaseOutCubic(p)) * scale;
        SetOval(Image_Glow, radius);
        SetAlpha(Image_Glow, GlowColor, 0.4f * (1f - p));
    }

    // 닿는 순간의 번쩍임
    private void UpdateFlash(float scale)
    {
        float p = Mathf.InverseLerp(_impactTime, _impactTime + 0.42f, _elapsed);
        bool visible = p > 0f && p < 1f;
        SetVisible(Image_Flash, visible);

        if (visible == false)
        {
            return;
        }

        float radius = (10f + 44f * EaseOutCubic(p)) * scale;
        SetOval(Image_Flash, radius);
        SetAlpha(Image_Flash, FlashColor, 0.85f * (1f - p));
    }

    // 위에서 가속하며 떨어지는 물방울
    private void UpdateDrop(float scale, float height)
    {
        float p = Mathf.InverseLerp(_dropStartTime, _impactTime, _elapsed);
        bool visible = p > 0f && p < 1f;
        SetVisible(Image_Drop, visible);

        if (visible == false)
        {
            return;
        }

        float startY = height * 0.5f + 60f * scale;
        float endY = _impactOffset.y + 12f * scale;
        float y = Mathf.Lerp(startY, endY, p * p);

        RectTransform rt = Image_Drop.rectTransform;
        rt.anchoredPosition = new Vector2(_impactOffset.x, y);
        rt.sizeDelta = new Vector2(20f * scale, 40f * scale * Mathf.Lerp(0.6f, 1.5f, p));
        SetAlpha(Image_Drop, DropColor, 1f);
    }

    private void SetOval(Image image, float radius)
    {
        RectTransform rt = image.rectTransform;
        rt.anchoredPosition = _impactOffset;
        rt.sizeDelta = new Vector2(radius * 2f, radius * 2f * _squash);
    }

    // 보이기 전에는 클릭도 막는다.
    private static void SetGroup(CanvasGroup group, float alpha)
    {
        if (group != null)
        {
            group.alpha = alpha;
            group.blocksRaycasts = alpha >= 0.99f;
        }
    }

    private static void SetVisible(Image image, bool visible)
    {
        if (image != null)
        {
            image.enabled = visible;
        }
    }

    private static void SetAlpha(Image image, Color baseColor, float alpha)
    {
        baseColor.a = alpha;
        image.color = baseColor;
    }

    private static float EaseOutCubic(float x)
    {
        x = Mathf.Clamp01(x);
        float inv = 1f - x;
        return 1f - inv * inv * inv;
    }

    private static float SmoothStep(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x);
    }
}
