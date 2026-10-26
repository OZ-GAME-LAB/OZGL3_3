using UnityEngine;

public enum UIRootType
{
    None = 0,
    BackgroundUI,
    MainUI,
    ContentUI,
    PopupUI,
    VeryFrontUI
}

public enum UIType
{
    GameStartUI,
    MainUI,
    SimplePopupUI,
    MainHUD,
    StatusUI,
    FadeUI
}

public static class UIManagerExtension
{
    public static string GetUIPath(this UIManager uiManager, UIRootType uiRootType, UIType uiType)
    {
        string path = string.Empty; // "" == string.Empty

        path = $"UI/{uiRootType}/{uiType}";
        return path;
    }

    public static void ShowStartupUIOnGameStart(this UIManager uiManager)
    {
        uiManager.OpenGameStartUI();
    }

    public static void OpenGameStartUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.ContentUI, UIType.GameStartUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void CloseGameStartUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.ContentUI, UIType.GameStartUI);
    }

    public static void OpenMainUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.MainUI, UIType.MainUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void CloseMainUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.MainUI, UIType.MainUI);
    }

    public static void OpenSimplePopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.SimplePopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void CloseSimplePopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.SimplePopupUI);
    }

    public static void OpenStatusUI(this UIManager uiManager, PlayerStatSet playerStats)
    {
        if (uiManager.IsOpenUI(UIType.StatusUI) == true)
        {
            uiManager.CloseStatusUI();
            return;
        }

        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.StatusUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }

        (uiBase as StatusUI)?.Init(PlayerStatSet.Instance);
    }

    public static void CloseStatusUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.StatusUI);
    }

        public static void OpenMainHUD(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.MainUI, UIType.MainHUD);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void CloseMainHUDI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.MainUI, UIType.MainHUD);
    }

    public static void OpenFadeUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.VeryFrontUI, UIType.FadeUI);
        FadeUI fadeUI = uiBase as FadeUI;

        if (fadeUI == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }

        fadeUI.Play();
    }

    public static void CloseFadeUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.VeryFrontUI, UIType.FadeUI);
    }
}
