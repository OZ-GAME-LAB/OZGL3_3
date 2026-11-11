using UnityEngine;

public class MainHUD : UIBase
{
    private HeartPanel _heartPanel;
    private SkillGaugePanel _skillGaugePanel;
    private DashPanel _dashPanel;
    private SkillPanel _skillPanel;
    private PlayerStatus _deathStatus;

    private void Awake()
    {
        _heartPanel = GetComponentInChildren<HeartPanel>(true);
        _skillGaugePanel = GetComponentInChildren<SkillGaugePanel>(true);
        _dashPanel = GetComponentInChildren<DashPanel>(true);
        _skillPanel = GetComponentInChildren<SkillPanel>(true);
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

        // 플레이어가 준비될 때까지 SkillPanel이 스스로 기다린다.
        if (_skillPanel != null)
        {
            _skillPanel.Begin();
        }

        TrySubscribeDeath();
    }

    private void OnDisable()
    {
        UnsubscribeDeath();

        if (_skillPanel != null)
        {
            _skillPanel.Release();
        }
    }

    // 플레이어가 준비되는 시점이 늦을 수 있어서, 아직 구독하지 못했다면 계속 시도한다.
    private void TrySubscribeDeath()
    {
        PlayerStatus status = PlayerStatus.Instance;
        if (status == null || status == _deathStatus)
        {
            return;
        }

        UnsubscribeDeath();
        _deathStatus = status;
        _deathStatus.OnDied += HandlePlayerDied;

        if (_deathStatus.IsDead == true)
        {
            HandlePlayerDied();
        }
    }

    private void UnsubscribeDeath()
    {
        if (_deathStatus != null)
        {
            _deathStatus.OnDied -= HandlePlayerDied;
        }

        _deathStatus = null;
    }

    private void HandlePlayerDied()
    {
        WorldMapUI.Close();
        DeathUI.Open();
    }

    private void Update()
    {
        TrySubscribeDeath();

        // 사망 창이 떠 있는 동안에는 다른 창을 열지 않는다.
        if (DeathUI.IsOpen == true)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (WorldMapUI.IsOpen == true)
            {
                WorldMapUI.Close();
            }
            else if (UIManager.Instance.IsOpenUI(UIType.PauseUI))
            {
                UIManager.Instance.ClosePauseUI();
            }
            else if (UIManager.Instance.IsOpenUI(UIType.StatusUI))
            {
                UIManager.Instance.CloseStatusUI();
            }
            else
            {
                UIManager.Instance.OpenPauseUI();
            }

            return;
        }

        // 일시정지 중에는 다른 창을 열지 않는다.
        if (UIManager.Instance.IsOpenUI(UIType.PauseUI))
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            WorldMapUI.Toggle();
            return;
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            UIManager.Instance.OpenStatusUI(PlayerStatSet.Instance);
        }
    }
}