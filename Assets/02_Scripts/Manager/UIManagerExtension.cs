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
    FadeUI,
    PauseUI,
    SettingPopupUI,
    AreaSpriteUI,
    ShopUI,
    ArtifactUI,
    DialoagueUI
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

    public static bool OpenShopUI(this UIManager uiManager, NpcManager npc = null, string npcId = null, Sprite portrait = null)
    {
        npc ??= GameManager.Npc;
        if (!npc.TryOpen(NpcType.Shop, npcId, portrait))
        {
            return false;
        }

        UIBase uiBase = uiManager.OpenUI(UIRootType.ContentUI, UIType.ShopUI);
        if (uiBase is not ShopUI ui)
        {
            uiManager.CloseUI(UIRootType.ContentUI, UIType.ShopUI);
            npc.Close();
            Logger.LogError("ShopUI에 ShopUI 컴포넌트와 UI 참조를 연결해야 합니다.");
            return false;
        }

        ui.Init(npc);
        return true;
    }

    public static void CloseShopUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.ContentUI, UIType.ShopUI);
    }

    public static bool OpenArtifactUI(this UIManager uiManager, NpcManager npc = null, string npcId = null, Sprite portrait = null)
    {
        npc ??= GameManager.Npc;
        if (!npc.TryOpen(NpcType.Artifact, npcId, portrait))
        {
            return false;
        }

        UIBase uiBase = uiManager.OpenUI(UIRootType.ContentUI, UIType.ArtifactUI);
        if (uiBase is not ArtifactUI ui)
        {
            uiManager.CloseUI(UIRootType.ContentUI, UIType.ArtifactUI);
            npc.Close();
            Logger.LogError("ArtifactUI에 ArtifactUI 컴포넌트와 UI 참조를 연결해야 합니다.");
            return false;
        }

        ui.Init(npc);
        return true;
    }

    public static void CloseArtifactUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.ContentUI, UIType.ArtifactUI);
    }

    public static bool OpenDialoaguetUI(this UIManager uiManager, NpcManager npc = null, string npcId = null, Sprite portrait = null)
    {
        npc ??= GameManager.Npc;
        if (!npc.TryOpen(NpcType.Story, npcId, portrait))
        {
            return false;
        }

        return ShowDialogueUI(uiManager, npc);
    }

    public static bool OpenDialogueGroupUI(this UIManager uiManager, NpcManager npc, string dialogueGroupId, Sprite portrait = null)
    {
        if (!npc.TryOpenGroup(dialogueGroupId, portrait))
        {
            return false;
        }

        return ShowDialogueUI(uiManager, npc);
    }

    private static bool ShowDialogueUI(UIManager uiManager, NpcManager npc)
    {
        UIBase uiBase = uiManager.OpenUI(UIRootType.ContentUI, UIType.DialoagueUI);
        if (uiBase is not DialogueUI ui)
        {
            uiManager.CloseUI(UIRootType.ContentUI, UIType.DialoagueUI);
            npc.Close();
            Logger.LogError("DialoaguetUI에 DialogueUI 컴포넌트와 UI 참조를 연결해야 합니다.");
            return false;
        }

        ui.Init(npc);
        return true;
    }

    public static void CloseDialoagueUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.ContentUI, UIType.DialoagueUI);
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
        uiManager.CloseUI(UIRootType.PopupUI, UIType.FadeUI);
    }

    public static void OpenPauseUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.PauseUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void ClosePauseUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.VeryFrontUI, UIType.PauseUI);
    }

    public static void OpenSettingPopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.SettingPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void CloseSettingPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.SettingPopupUI);
    }

    public static void OpenAreaSpriteUI(this UIManager uiManager, string id)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.AreaSpriteUI);
        AreaSpriteUI areaSpriteUI = uiBase as AreaSpriteUI;

        if (areaSpriteUI == null)
        {
            Logger.LogError("AreaSpriteUI가 생성되지 않았습니다.");
            return;
        }

        areaSpriteUI.Init(id);
    }

    public static void CloseAreaSpriteUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.AreaSpriteUI);
    }
}
