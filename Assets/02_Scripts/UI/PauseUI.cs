using UnityEngine;

public class PauseUI : UIBase
{
    [SerializeField] private UIButton Button_ReStart;
    [SerializeField] private UIButton Button_Setting;
    [SerializeField] private UIButton Button_Quit;

    private void Awake()
    {
        Button_ReStart.BindOnClickButtonEvent(OnClickReStart);
        Button_Setting.BindOnClickButtonEvent(OnClickSetting);
        Button_Quit.BindOnClickButtonEvent(OnClickQuit);
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    private void OnClickReStart()
    {
        UIManager.Instance.ClosePauseUI();
    }

    private void OnClickSetting()
    {
        // TODO(강혜창): SettingUI 만든 뒤 연결
    }

    private void OnClickQuit()
    {
        UIManager.Instance.ClosePauseUI();
        UIManager.Instance.OpenGameStartUI();
    }
}