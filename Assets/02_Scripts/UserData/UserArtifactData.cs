using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class UserArtifactData : IUserData
{
    public List<string> OwnedArtifactIds => _ownedArtifactIds;
    public List<string> EquippedArtifactIds => _equippedArtifactIds;
    public bool IsOverloadUnlocked { get => _isOverloadUnlocked; set => _isOverloadUnlocked = value; }

    [SerializeField] private List<string> _ownedArtifactIds = new();
    [SerializeField] private List<string> _equippedArtifactIds = new();
    [SerializeField] private bool _isOverloadUnlocked;

    private readonly string _path;

    public UserArtifactData(string saveDirectory)
    {
        _path = Path.Combine(saveDirectory, "ArtifactData.json");
    }

    public void SetDefaultData()
    {
        _ownedArtifactIds.Clear();
        _equippedArtifactIds.Clear();
        _isOverloadUnlocked = false;
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
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path)));
            File.WriteAllText(_path, JsonUtility.ToJson(this, true));
            return true;
        }
        catch (Exception exception)
        {
            Logger.LogError($"{GetType().Name}: Save failed ({exception.Message})");
            return false;
        }
    }
}
