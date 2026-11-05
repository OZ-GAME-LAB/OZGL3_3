using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

[Serializable]
public class UserSlotMetaData
{
    public static string FileName => "SlotMeta.json";

    [Serializable]
    public class Slot
    {
        public int Index => _index;
        public int Currency { get => _currency; set => _currency = value; }
        public double PlayTimeSeconds { get => _playTimeSeconds; set => _playTimeSeconds = value; }
        public DateTime CreatedAtUtc => DateTime.Parse(_createdAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        [SerializeField] private int _index;
        [SerializeField] private int _currency;
        [SerializeField] private double _playTimeSeconds;
        [SerializeField] private string _createdAtUtc;

        public Slot(int index)
        {
            _index = index;
            _createdAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        }
    }

    [SerializeField] private List<Slot> _slots = new();

    private readonly string _path;

    public UserSlotMetaData(string saveDirectory)
    {
        _path = Path.Combine(saveDirectory, FileName);
    }

    public Slot GetSlot(int slot)
    {
        foreach (Slot meta in _slots)
        {
            if (meta.Index == slot)
            {
                return meta;
            }
        }

        return null;
    }

    public Slot CreateSlot(int slot)
    {
        Slot meta = new(slot);
        _slots.Add(meta);
        return meta;
    }

    public void DeleteSlot(int slot)
    {
        _slots.Remove(GetSlot(slot));
    }

    public bool LoadData()
    {
        _slots.Clear();
        if (!File.Exists(_path))
        {
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
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
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
