using TMPro;
using UnityEngine;

// 캐릭터 스텟 상태창: PlayerStats를 받아 스텟을 보여 준다.
// 값이 바뀌면(PlayerStats.OnStatsChanged) 스스로 다시 그린다.
public class StatusUI : UIBase
{
    [SerializeField] private TextMeshProUGUI Text_Attack;
    [SerializeField] private TextMeshProUGUI Text_Health;
    [SerializeField] private TextMeshProUGUI Text_Speed;
    [SerializeField] private TextMeshProUGUI Text_CrouchSpeed;
    [SerializeField] private TextMeshProUGUI Text_Jump;
    [SerializeField] private TextMeshProUGUI Text_Chaos;

    private PlayerStatSet _playerStats;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            UIManager.Instance.CloseStatusUI();
        }
    }

    private void OnDisable()
    {
        // UIManager는 UI를 파괴하지 않고 끄기만 하므로, 꺼질 때 구독을 반드시 해제한다.
        Unsubscribe();
        _playerStats = null;
    }

    // OnEnable은 항상 Init보다 먼저 실행되므로, 데이터가 필요한 처리는 여기서 시작한다.
    public void Init(PlayerStatSet playerStats)
    {
        Unsubscribe();

        if (playerStats == null)
        {
            Logger.LogError("StatUI: PlayerStats가 null입니다.");
            return;
        }

        _playerStats = playerStats;
        _playerStats.OnStatsChanged += OnChangedStats;

        // 이벤트는 값이 바뀔 때만 오므로, 시작할 때 현재 값을 직접 한 번 그려 준다.
        InitStat();
    }

    private void Unsubscribe()
    {
        if (_playerStats != null)
        {
            _playerStats.OnStatsChanged -= OnChangedStats;
        }
    }

    private void OnChangedStats()
    {
        InitStat();
    }

    private void InitStat()
    {
        Text_Attack.text = _playerStats.AttackDamage.ToString();
        Text_Health.text = _playerStats.MaxHealth.ToString();
        Text_Speed.text = _playerStats.WalkSpeed.ToString("0.##");
        Text_CrouchSpeed.text = _playerStats.CrouchSpeed.ToString("0.##");
        Text_Jump.text = _playerStats.JumpValue.ToString("0.##");
        Text_Chaos.text = _playerStats.MaxChaosGauge.ToString();
    }
}