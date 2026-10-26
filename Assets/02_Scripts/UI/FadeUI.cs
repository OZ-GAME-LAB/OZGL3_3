using System;
using System.Collections;
using UnityEngine;

public class FadeUI : UIBase
{
    public static event Action OnFadeBlack;

    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeOutDuration = 0.4f;
    [SerializeField] private float _holdDuration = 0.1f;
    [SerializeField] private float _fadeInDuration = 0.4f;

    private Coroutine _fadeCoroutine;

    private void OnDisable()
    {
        _fadeCoroutine = null;
    }

    public void Play()
    {
        // 이미 진행 중이면 무시한다.
        if (_fadeCoroutine != null)
        {
            return;
        }

        _fadeCoroutine = StartCoroutine(DoFadeInOut());
    }

    private IEnumerator DoFadeInOut()
    {
        _canvasGroup.blocksRaycasts = true;

        yield return DoFade(0f, 1f, _fadeOutDuration);

        OnFadeBlack?.Invoke();
        yield return new WaitForSecondsRealtime(_holdDuration);

        yield return DoFade(1f, 0f, _fadeInDuration);

        _canvasGroup.blocksRaycasts = false;
        _fadeCoroutine = null;

        UIManager.Instance.CloseFadeUI();
    }

    private IEnumerator DoFade(float startAlpha, float targetAlpha, float duration)
    {
        float elapsed = 0f;
        _canvasGroup.alpha = startAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        _canvasGroup.alpha = targetAlpha;
    }
}