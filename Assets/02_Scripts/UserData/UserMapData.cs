using System;
using System.IO;
using UnityEngine;

[Serializable]
public class UserMapData : IUserData
{
    [SerializeField] private string[] _visitedRoomKeys = Array.Empty<string>();
    [SerializeField] private bool _isRevealAll;

    private string _path;

    public event Action OnSaved;

    public UserMapData(string saveDirectory)
    {
        SetSaveDirectory(saveDirectory);
    }

    public void SetSaveDirectory(string saveDirectory)
    {
        _path = saveDirectory == null ? null : Path.Combine(saveDirectory, "MapData.json");
    }

    public void SetDefaultData()
    {
        _visitedRoomKeys = Array.Empty<string>();
        _isRevealAll = false;
        ApplyToWorldMap();
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
            ApplyToWorldMap();
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
            _visitedRoomKeys = WorldMapData.GetVisitedKeys();
            _isRevealAll = WorldMapData.IsRevealAll;
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

    private void ApplyToWorldMap()
    {
        WorldMapData.SetVisitedKeys(_visitedRoomKeys);
        WorldMapData.IsRevealAll = _isRevealAll;
    }
}
