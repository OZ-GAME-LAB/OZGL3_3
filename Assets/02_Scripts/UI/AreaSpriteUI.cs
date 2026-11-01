using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 구간 진입 이미지: 데이터의 경로로 스프라이트를 불러와 페이드로 보여 주고, 잠시 유지한 뒤 사라진다.
public class AreaSpriteUI : UIBase
{
    [SerializeField] private Image Image_Area;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeInDuration = 0.4f;
    [SerializeField] private float _holdDuration = 2f;       // 완전히 보인 뒤 유지 시간(초)
    [SerializeField] private float _fadeOutDuration = 0.5f;
    [SerializeField] private bool _isSetNativeSize = true;    // 원본 크기로 맞출지

    private Sequence _sequence;

    private void OnDisable()
    {
        // UIManager는 UI를 파괴하지 않고 끄기만 하므로, 꺼질 때 트윈을 반드시 끈다.
        _sequence?.Kill();
        _sequence = null;
    }

    // OnEnable은 항상 Init보다 먼저 실행되므로, 데이터가 필요한 처리는 여기서 시작한다.
    public void Init(string id)
    {
        if (GameManager.Instance == null)
        {
            Logger.LogError("AreaSpriteUI: GameManager가 씬에 없습니다.");
            return;
        }

        if (GameManager.DataTable.TryGetAreaSprite(id, out AreaTextData data) == false)
        {
            Logger.LogError($"AreaSpriteUI: AreaSpriteData에서 '{id}'를 찾을 수 없습니다.");
            UIManager.Instance.CloseAreaSpriteUI();
            return;
        }

        Sprite sprite = Resources.Load<Sprite>(data.SpritePath);

        if (sprite == null)
        {
            Logger.LogError($"AreaSpriteUI: 스프라이트를 찾을 수 없습니다: Resources/{data.SpritePath}");
            UIManager.Instance.CloseAreaSpriteUI();
            return;
        }

        // 이미 열려 있는 상태에서 다시 불리면 이전 연출을 끊고 새로 시작한다.
        _sequence?.Kill();

        Image_Area.sprite = sprite;

        if (_isSetNativeSize == true)
        {
            Image_Area.SetNativeSize();
        }

        _canvasGroup.alpha = 0f;

        _sequence = DOTween.Sequence()
            .SetUpdate(true)    // 일시정지 중에도 진행
            .Append(_canvasGroup.DOFade(1f, _fadeInDuration))
            .AppendInterval(_holdDuration)
            .Append(_canvasGroup.DOFade(0f, _fadeOutDuration))
            .OnComplete(OnEndSequence);
    }

    private void OnEndSequence()
    {
        UIManager.Instance.CloseAreaSpriteUI();
    }
}