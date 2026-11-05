using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class UserDialogueData : IUserData
{
    public int StoryStage { get => _storyStage; set => _storyStage = value; }
    public IReadOnlyCollection<string> Flags => _flags;
    public IReadOnlyCollection<string> Events => _events;
    public List<string> CompletedDialogueGroupIds => _completedDialogueGroupIds;

    [SerializeField] private int _storyStage;
    [SerializeField] private List<string> _flags = new();
    [SerializeField] private List<string> _events = new();
    [SerializeField] private List<string> _completedDialogueGroupIds = new();

    private string _path;

    public event Action OnSaved;

    public UserDialogueData(string saveDirectory)
    {
        SetSaveDirectory(saveDirectory);
    }

    public void SetSaveDirectory(string saveDirectory)
    {
        _path = saveDirectory == null ? null : Path.Combine(saveDirectory, "DialogueData.json");
    }

    public void SetDefaultData()
    {
        _storyStage = 0;
        _flags.Clear();
        _events.Clear();
        _completedDialogueGroupIds.Clear();
    }

    public void SetFlag(string id, bool valueAdd = true)
    {
        if (!valueAdd)
        {
            _flags.Remove(id);
        }
        else if (!_flags.Contains(id))
        {
            _flags.Add(id);
        }
    }

    public void SetEvent(string id, bool valueAdd = true)
    {
        if (!valueAdd)
        {
            _events.Remove(id);
        }
        else if (!_events.Contains(id))
        {
            _events.Add(id);
        }
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
