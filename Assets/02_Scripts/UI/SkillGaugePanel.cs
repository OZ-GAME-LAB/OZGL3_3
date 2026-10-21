using UnityEngine;
using UnityEngine.UI;

// 스킬 게이지: 현재 / 최대 비율로 바를 채운다.
public class SkillGaugePanel : MonoBehaviour
{
    [SerializeField] private Image Image_SkillGauge;    // Image Type: Filled

    private PlayerStatus _playerStatus;

    private void OnDisable()
    {
        // UIManager는 UI를 파괴하지 않고 끄기만 하므로, 꺼질 때 구독을 반드시 해제한다.
        DoUnsubscribe();
        _playerStatus = null;
    }

    public void Init(PlayerStatus playerStatus)
    {
        DoUnsubscribe();

        if (playerStatus == null)
        {
            Logger.LogError("SkillGaugePanel: PlayerStatus가 null입니다.");
            return;
        }

        _playerStatus = playerStatus;
        _playerStatus.OnSkillGaugeChanged += OnChangedSkillGauge;

        // 이벤트는 값이 바뀔 때만 오므로, 시작할 때 현재 값을 직접 한 번 그린다.
        DoRefresh(_playerStatus.CurrentSkillGauge, _playerStatus.MaxSkillGauge);
    }

    private void DoUnsubscribe()
    {
        if (_playerStatus != null)
        {
            _playerStatus.OnSkillGaugeChanged -= OnChangedSkillGauge;
        }
    }

    private void OnChangedSkillGauge(int current, int max)
    {
        DoRefresh(current, max);
    }

    private void DoRefresh(int current, int max)
    {
        Image_SkillGauge.fillAmount = max > 0 ? (float)current / max : 0f;
    }
}