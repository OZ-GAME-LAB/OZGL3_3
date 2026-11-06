using System;
using DG.Tweening;
using UnityEngine;

public class FadeUI : UIBase
{
    public static event Action OnFadeBlack;

    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeOutDuration = 0.4f;
    [SerializeField] private float _holdDuration = 0.1f;
    [SerializeField] private float _fadeInDuration = 0.4f;

    private Sequence _sequence;

    private void OnDisable()
    {
        // UI는 파괴되지 않고 SetActive로 꺼지므로 여기서 트윈을 정리한다.
        KillSequence();
        _canvasGroup.blocksRaycasts = false;
    }

    public void Play()
    {
        // 이미 진행 중이면 무시한다.
        if (_sequence != null && _sequence.IsActive() == true)
        {
            return;
        }

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = true;

        _sequence = DOTween.Sequence();
        _sequence.SetUpdate(true);          // Time.timeScale = 0 이어도 동작
        _sequence.SetLink(gameObject);
        _sequence.Append(_canvasGroup.DOFade(1f, _fadeOutDuration).SetEase(Ease.Linear));
        _sequence.AppendCallback(InvokeFadeBlack);
        _sequence.AppendInterval(_holdDuration);
        _sequence.Append(_canvasGroup.DOFade(0f, _fadeInDuration).SetEase(Ease.Linear));
        _sequence.OnComplete(OnFadeComplete);
    }

    private void InvokeFadeBlack()
    {
        OnFadeBlack?.Invoke();
    }

    private void OnFadeComplete()
    {
        _sequence = null;
        _canvasGroup.blocksRaycasts = false;
        UIManager.Instance.CloseFadeUI();
    }

    private void KillSequence()
    {
        if (_sequence != null)
        {
            _sequence.Kill();
            _sequence = null;
        }
    }
}