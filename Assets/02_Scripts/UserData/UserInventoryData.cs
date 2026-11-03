using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class UserInventoryData : IUserData
{
    [Serializable]
    public class Item
    {
        [SerializeField] private string _id;
        [SerializeField] private int _count;

        public string Id => _id;
        public int Count { get => _count; set => _count = value; }

        public Item(string id, int count)
        {
            _id = id;
            _count = count;
        }
    }

    [SerializeField] private int _currency;
    [SerializeField] private List<Item> _items = new();

    public int Currency { get => _currency; set => _currency = value; }
    public IReadOnlyList<Item> Items => _items;

    private readonly string _path;

    public UserInventoryData(string saveDirectory)
    {
        _path = Path.Combine(saveDirectory, "InventoryData.json");
    }

    public void SetDefaultData()
    {
        _currency = 0;
        _items.Clear();
    }

    public int GetItemCount(string id)
    {
        foreach (Item item in _items)
        {
            if (item.Id == id)
            {
                return item.Count;
            }
        }

        return 0;
    }

    public void AddItem(string id, int amount)
    {
        foreach (Item item in _items)
        {
            if (item.Id == id)
            {
                item.Count = checked(item.Count + amount);
                return;
            }
        }

        _items.Add(new Item(id, amount));
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
