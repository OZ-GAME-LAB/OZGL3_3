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

        // 시작할 때 현재 구간 로고를 보여 준다.
        //TODO 강혜창 : 플레이어 생성 시점 확인 후 재설정 
        string regionId = GameManager.Area.CurrentRegionId;

        if (string.IsNullOrEmpty(regionId) == false)
        {
            UIManager.Instance.OpenAreaSpriteUI(regionId);
        }
    }

    private void OnClickSetting()
    {
        UIManager.Instance.OpenSettingPopupUI();

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
