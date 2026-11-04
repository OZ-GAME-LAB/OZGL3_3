using UnityEditor;

public class SaveLoadTool : Editor
{
    [MenuItem("Tools/UserData/Save")]
    public static void Save()
    {
        GameManager.UserData.SaveAllUserData();
    }

    [MenuItem("Tools/UserData/Load")]
    public static void Load()
    {
        GameManager.UserData.LoadAllUserData();
    }

    [MenuItem("Tools/UserData/SetDefault")]
    public static void SetDefault()
    {
        GameManager.UserData.SetAllDefaultUserData();
    }
}
