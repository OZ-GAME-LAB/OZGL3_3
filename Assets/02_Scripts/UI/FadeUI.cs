using System;
using System.Collections;
using UnityEngine;

public class FadeUI : UIBase
{
    [SerializeField] private CanvasGroup _canvasGroup;

    private Coroutine _fadeCoroutine;

    public float CurrentAlpha => _canvasGroup.alpha;

    private void OnDisable()
    {
        _fadeCoroutine = null;
    }

    public void Play(float startAlpha, float targetAlpha, float duration, Action onComplete)
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(DoFade(startAlpha, targetAlpha, duration, onComplete));
    }

    private IEnumerator DoFade(float startAlpha, float targetAlpha, float duration, Action onComplete)
    {
        _canvasGroup.alpha = startAlpha;
        _canvasGroup.blocksRaycasts = true;    // 페이드 중에는 클릭을 막는다.

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        _canvasGroup.alpha = targetAlpha;
        _canvasGroup.blocksRaycasts = targetAlpha > 0f;    // 검은 상태에서는 계속 막는다.
        _fadeCoroutine = null;

        onComplete?.Invoke();
    }
}