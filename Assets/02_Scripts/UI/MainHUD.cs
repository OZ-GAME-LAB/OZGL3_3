using UnityEngine;

public class MainHUD : UIBase
{
    private HeartPanel _heartPanel;
    private SkillGaugePanel _skillGaugePanel;

    private PlayerStats playerStats;

    private void Awake()
    {
        _heartPanel = GetComponentInChildren<HeartPanel>();
        _skillGaugePanel = GetComponentInChildren<SkillGaugePanel>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            UIManager.Instance.OpenStatusUI(this.playerStats);
        }
    }

    private void OnEnable()
    {
        PlayerStatus playerStatus = PlayerStatus.Instance;

        if (playerStatus == null)
        {
            Logger.LogError("MainHUD : PlayerStaus가 없습니다.");
            return;
        }
    }
}
