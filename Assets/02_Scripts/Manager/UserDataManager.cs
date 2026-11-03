using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UserDataManager
{
    public List<IUserData> UserDataList { get; } = new();
    public event Action OnChanged;

    public void Init()
    {
        string saveDirectory = Path.Combine(Application.dataPath, "UserData");

        UserDataList.Clear();

        UserDataList.Add(new UserArtifactData(saveDirectory));
        UserDataList.Add(new UserShopData(saveDirectory));
        UserDataList.Add(new UserDialogueData(saveDirectory));
        UserDataList.Add(new UserInventoryData(saveDirectory));

        SetAllDefaultUserData();
    }

    public void SetAllDefaultUserData()
    {
        foreach (IUserData userData in UserDataList)
        {
            userData.SetDefaultData();
        }

        OnChanged?.Invoke();
    }

    public bool LoadAllUserData()
    {
        bool success = true;
        foreach (IUserData userData in UserDataList)
        {
            success &= userData.LoadData();
        }

        OnChanged?.Invoke();
        return success;
    }

    public bool SaveAllUserData()
    {
        bool success = true;
        foreach (IUserData userData in UserDataList)
        {
            success &= userData.SaveData();
        }

        OnChanged?.Invoke();
        return success;
    }

    public T GetUserData<T>() where T : class, IUserData
    {
        foreach (IUserData userData in UserDataList)
        {
            if (userData is T data)
            {
                return data;
            }
        }

        Logger.LogError($"{typeof(T).Name} 타입의 데이터가 존재하지 않습니다.");
        return null;
    }
}
