using System;
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
}

public static class UIManagerExtension
{
    public static string GetUIPath(this UIManager uiManager, UIRootType uiRootType, UIType uiType)
    {
        string path = string.Empty; // "" == string.Empty

        path = $"Prefab/UI/{uiRootType}/{uiType}";
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
}
