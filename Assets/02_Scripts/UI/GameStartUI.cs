using UnityEngine;

public class GameStartUI : UIBase
{
    [SerializeField] private UIButton Button_Start;
    [SerializeField] private UIButton Button_Setting;
    [SerializeField] private UIButton Button_Exit;

    private void OnEnable()
    {
        Button_Start.BindOnClickButtonEvent(OnClickStart);
        Button_Setting.BindOnClickButtonEvent(OnClickSetting);
        Button_Exit.BindOnClickButtonEvent(OnClickExit);
    }

    private void OnDisable()
    {
        Button_Start.UnBindAllOnClickButtonEvent();
        Button_Setting.UnBindAllOnClickButtonEvent();
        Button_Exit.UnBindAllOnClickButtonEvent();
    }

    private void OnClickStart()
    {
        UIManager.Instance.CloseGameStartUI();
        UIManager.Instance.OpenMainHUD();
    }

    private void OnClickSetting()
    {
        //TODO(강혜창) : 셋팅 팝업 UI 열기
    }

    private void OnClickExit()
    {
        //TODO(강혜창) : 게임 매니저에서 받아와서 종료로 바꾸기
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
