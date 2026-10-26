using UnityEngine;

public class MainHUD : UIBase
{
    private HeartPanel _heartPanel;
    private SkillGaugePanel _skillGaugePanel;
    private DashPanel _dashPanel;

    private void Awake()
    {
        _heartPanel = GetComponentInChildren<HeartPanel>(true);
        _skillGaugePanel = GetComponentInChildren<SkillGaugePanel>(true);
        _dashPanel = GetComponentInChildren<DashPanel>(true);
    }

    private void OnEnable()
    {
        PlayerStatus playerStatus = PlayerStatus.Instance;

        if (playerStatus == null)
        {
            Logger.LogError("MainHUD: PlayerStatus가 없습니다.");
        }
        else
        {
            if (_heartPanel != null)
            {
                _heartPanel.Init(playerStatus);
            }

            if (_skillGaugePanel != null)
            {
                _skillGaugePanel.Init(playerStatus);
            }
        }

        // TODO(강혜창): PlayerController가 싱글톤이 되면 Find 대신 Instance 사용
        if (_dashPanel != null)
        {
            _dashPanel.Init(FindFirstObjectByType<PlayerController>());
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            UIManager.Instance.OpenStatusUI(PlayerStats.Instance);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.OpenPauseUI();
        }
    }
}