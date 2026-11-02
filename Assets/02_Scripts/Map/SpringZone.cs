using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class SpringZone : MonoBehaviour
{
    [Header("Spring Visual")]
    [FormerlySerializedAs("_transform_SpringVisual")]
    [SerializeField] private Transform Transform_SpringVisual;

    [Header("Press Animation")]
    [SerializeField, Min(0.01f)] private float _pressDuration = 0.08f;
    [SerializeField, Min(0.01f)] private float _returnDuration = 0.12f;
    [SerializeField, Range(0.1f, 1f)] private float _pressedScaleY = 0.65f;

    private Vector3 _originalScale;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isAnimating;
    private int _animationVersion;

    private void Awake()
    {
        if (Transform_SpringVisual == null)
        {
            Logger.LogError(
                "SpringZone: Spring Visual 자식 Transform을 Inspector에 연결하세요.");
            enabled = false;
            return;
        }

        _originalScale = Transform_SpringVisual.localScale;
    }

    private void OnEnable()
    {
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponentInParent<PlayerController>() == null)
        {
            return;
        }

        DoPlayPressAnimation();
    }

    private void DoPlayPressAnimation()
    {
        if (_isAnimating || _cancellationTokenSource == null)
        {
            return;
        }

        _isAnimating = true;
        int animationVersion = ++_animationVersion;
        DoPressAnimationAsync(
            _cancellationTokenSource.Token,
            animationVersion).Forget();
    }

    private async UniTask DoPressAnimationAsync(
        CancellationToken cancellationToken,
        int animationVersion)
    {
        Vector3 pressedScale = new Vector3(
            _originalScale.x,
            _originalScale.y * _pressedScaleY,
            _originalScale.z);

        try
        {
            await DoAnimateScaleAsync(
                _originalScale,
                pressedScale,
                _pressDuration,
                cancellationToken);

            await DoAnimateScaleAsync(
                pressedScale,
                _originalScale,
                _returnDuration,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // 컴포넌트 비활성화 시 애니메이션 취소
        }
        finally
        {
            if (animationVersion == _animationVersion)
            {
                if (this != null &&
                    isActiveAndEnabled &&
                    Transform_SpringVisual != null)
                {
                    Transform_SpringVisual.localScale = _originalScale;
                }

                _isAnimating = false;
            }
        }
    }

    private async UniTask DoAnimateScaleAsync(
        Vector3 startScale,
        Vector3 targetScale,
        float duration,
        CancellationToken cancellationToken)
    {
        if (duration <= 0f)
        {
            Transform_SpringVisual.localScale = targetScale;
            return;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            cancellationToken.ThrowIfCancellationRequested();

            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
            float easedTime = Mathf.SmoothStep(0f, 1f, normalizedTime);

            Transform_SpringVisual.localScale = Vector3.LerpUnclamped(
                startScale,
                targetScale,
                easedTime);

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        Transform_SpringVisual.localScale = targetScale;
    }

    private void OnDisable()
    {
        // 실행 중이던 애니메이션을 무효화한다.
        _animationVersion++;

        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        _isAnimating = false;

        if (Transform_SpringVisual != null)
        {
            Transform_SpringVisual.localScale = _originalScale;
        }
    }
}
