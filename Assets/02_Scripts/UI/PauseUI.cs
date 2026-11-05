using UnityEngine;

public class PauseUI : UIBase
{
    [SerializeField] private UIButton Button_ReStart;
    [SerializeField] private UIButton Button_Setting;
    [SerializeField] private UIButton Button_Quit;

    private void OnEnable()
    {
        GameManager.UserData.EndPlaySession();
        Time.timeScale = 0f;

        Button_ReStart.BindOnClickButtonEvent(OnClickReStart);
        Button_Setting.BindOnClickButtonEvent(OnClickSetting);
        Button_Quit.BindOnClickButtonEvent(OnClickQuit);
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        if (!UIManager.Instance.IsOpenUI(UIType.GameStartUI))
        {
            GameManager.UserData.BeginPlaySession();
        }

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
        if (GameManager.UserData.CurrentSlot >= 0 && !GameManager.UserData.SaveAllUserData())
        {
            SimplePopup popup = UIManager.Instance.OpenPopupUI(UIType.SimplePopupUI) as SimplePopup;
            popup?.Init("저장하지 못했습니다. 다시 시도해 주세요.");
            return;
        }

        UIManager.Instance.OpenGameStartUI();
        UIManager.Instance.ClosePauseUI();
    }
}
