using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class UserShopData : IUserData
{
    public List<string> PurchasedShopIds => _purchasedShopIds;

    [SerializeField] private List<string> _purchasedShopIds = new();

    private string _path;

    public event Action OnSaved;

    public UserShopData(string saveDirectory)
    {
        SetSaveDirectory(saveDirectory);
    }

    public void SetSaveDirectory(string saveDirectory)
    {
        _path = saveDirectory == null ? null : Path.Combine(saveDirectory, "ShopData.json");
    }

    public void SetDefaultData()
    {
        _purchasedShopIds.Clear();
    }

    public bool LoadData()
    {
        if (!File.Exists(_path))
        {
            SetDefaultData();
            return true;
        }

        try
        {
            JsonUtility.FromJsonOverwrite(File.ReadAllText(_path), this);
            return true;
        }
        catch (Exception exception)
        {
            Logger.LogError($"{GetType().Name}: Load failed ({exception.Message})");
            return false;
        }
    }

    public bool SaveData()
    {
        if (_path == null)
        {
            return false;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path)));
            File.WriteAllText(_path, JsonUtility.ToJson(this, true));
            OnSaved?.Invoke();
            return true;
        }
        catch (Exception exception)
        {
            Logger.LogError($"{GetType().Name}: Save failed ({exception.Message})");
            return false;
        }
    }
}
