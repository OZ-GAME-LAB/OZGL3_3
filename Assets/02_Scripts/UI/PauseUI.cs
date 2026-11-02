using UnityEngine;

public class PauseUI : UIBase
{
    [SerializeField] private UIButton Button_ReStart;
    [SerializeField] private UIButton Button_Setting;
    [SerializeField] private UIButton Button_Quit;

    private void OnEnable()
    {
        Time.timeScale = 0f;

        Button_ReStart.BindOnClickButtonEvent(OnClickReStart);
        Button_Setting.BindOnClickButtonEvent(OnClickSetting);
        Button_Quit.BindOnClickButtonEvent(OnClickQuit);
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;

        Button_ReStart.UnBindAllOnClickButtonEvent();
        Button_Setting.UnBindAllOnClickButtonEvent();
        Button_Quit.UnBindAllOnClickButtonEvent();
    }

    private void OnClickReStart()
    {
        UIManager.Instance.ClosePauseUI();
    }

    private void OnClickSetting()
    {
        UIManager.Instance.OpenSettingPopupUI();
    }

    private void OnClickQuit()
    {
        UIManager.Instance.ClosePauseUI();
        UIManager.Instance.OpenGameStartUI();
    }
}