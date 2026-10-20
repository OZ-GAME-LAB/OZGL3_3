using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;

public class SimplePopup : UIBase
{
    [SerializeField] private TextMeshProUGUI Text_Message;

    [Header("Init에서 표시 시간을 넘기지 않았을 때 사용하는 기본 시간 (초)")]
    [SerializeField] private float _defaultDurationSeconds = 1.5f;

    private CancellationTokenSource _closeCts;

    private void OnDisable()
    {
        // UIManager는 UI를 파괴하지 않고 끄기만 하므로, 꺼질 때 대기를 반드시 취소한다.
        DoCancelClose();
    }

    // OnEnable은 항상 Init보다 먼저 실행되므로, 데이터가 필요한 처리는 여기서 시작한다.
    public void Init(string message, float durationSeconds = 0f)
    {
        Text_Message.text = message;

        // 이미 열려 있는 팝업에 Init이 다시 불리면 이전 대기를 취소하고 새로 시작한다.
        DoCancelClose();

        float duration = durationSeconds > 0f ? durationSeconds : _defaultDurationSeconds;
        DoCloseAfterAsync(duration).Forget();
    }

    private async UniTask DoCloseAfterAsync(float seconds)
    {
        _closeCts = new CancellationTokenSource();
        CancellationToken token = _closeCts.Token;

        // 일시정지(timeScale 0) 중에도 닫히도록 ignoreTimeScale을 켠다.
        bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(seconds), ignoreTimeScale: true, cancellationToken: token)
            .SuppressCancellationThrow();

        if (isCanceled == true)
        {
            return;
        }

        UIManager.Instance.CloseSimplePopupUI();
    }

    private void DoCancelClose()
    {
        if (_closeCts == null)
        {
            return;
        }

        _closeCts.Cancel();
        _closeCts.Dispose();
        _closeCts = null;
    }
}