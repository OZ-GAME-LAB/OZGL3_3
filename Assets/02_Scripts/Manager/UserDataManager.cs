using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UserDataManager
{
    public static int SlotCount => 3;

    public List<IUserData> UserDataList { get; } = new();
    public int CurrentSlot { get; private set; } = -1;
    public bool IsPlaySessionActive => _playStartTime.HasValue;
    public event Action OnChanged;

    private string _saveDirectory;
    private UserSlotMetaData _slotMetaData;
    private UserSlotMetaData.Slot _currentSlotMeta;
    private bool _isSavingAll;
    private double? _playStartTime;

    public void Init(string saveDirectory = null)
    {
        _saveDirectory = Path.GetFullPath(saveDirectory ?? Path.Combine(Application.dataPath, "UserData"));
        _slotMetaData = new UserSlotMetaData(_saveDirectory);
        if (!_slotMetaData.LoadData())
        {
            _slotMetaData = null;
        }

        UserDataList.Clear();

        UserDataList.Add(new UserArtifactData(null));
        UserDataList.Add(new UserShopData(null));
        UserDataList.Add(new UserDialogueData(null));
        UserDataList.Add(new UserInventoryData(null));
        UserDataList.Add(new UserMapData(null));

        foreach (IUserData userData in UserDataList)
        {
            userData.OnSaved += OnUserDataSaved;
        }

        ClearCurrentSlot();
    }

    public bool HasSave(int slot)
    {
        return GetSlotMeta(slot) != null;
    }

    public UserSlotMetaData.Slot GetSlotMeta(int slot)
    {
        return _slotMetaData?.GetSlot(slot);
    }

    public bool CreateSlot(int slot)
    {
        if (_slotMetaData == null || HasSave(slot))
        {
            return false;
        }

        SetSlotDirectory(slot);
        _currentSlotMeta = _slotMetaData.CreateSlot(slot);
        SetAllDefaultUserData();
        if (SaveAllUserData())
        {
            return true;
        }

        // 생성에 실패한 슬롯을 선택 상태로 두거나 불완전한 파일을 남기지 않는다.
        DeleteSlot(slot);
        ClearCurrentSlot();
        return false;
    }

    public bool SelectSlot(int slot)
    {
        if (!HasSave(slot))
        {
            return false;
        }

        SetSlotDirectory(slot);
        _currentSlotMeta = GetSlotMeta(slot);
        if (LoadAllUserData())
        {
            return true;
        }

        ClearCurrentSlot();
        return false;
    }

    public bool DeleteSlot(int slot)
    {
        if (_slotMetaData == null)
        {
            return false;
        }

        try
        {
            string directory = GetSlotDirectory(slot);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }

            // dataPath 아래의 폴더를 삭제할 때 Unity 에셋 메타도 함께 제거한다.
            if (File.Exists(directory + ".meta"))
            {
                File.Delete(directory + ".meta");
            }

            _slotMetaData.DeleteSlot(slot);
            bool success = _slotMetaData.SaveData();

            if (CurrentSlot == slot)
            {
                ClearCurrentSlot();
            }

            OnChanged?.Invoke();
            return success;
        }
        catch (Exception exception)
        {
            Logger.LogError($"슬롯 {slot + 1}: Delete failed ({exception.Message})");
            return false;
        }
    }

    public void BeginPlaySession()
    {
        if (CurrentSlot >= 0 && !_playStartTime.HasValue)
        {
            _playStartTime = Time.realtimeSinceStartupAsDouble;
        }
    }

    public void EndPlaySession()
    {
        AccumulatePlayTime();
        _playStartTime = null;
    }

    private void AccumulatePlayTime()
    {
        if (_playStartTime.HasValue)
        {
            double currentTime = Time.realtimeSinceStartupAsDouble;
            _currentSlotMeta.PlayTimeSeconds += currentTime - _playStartTime.Value;
            _playStartTime = currentTime;
        }
    }

    private string GetSlotDirectory(int slot)
    {
        if (slot < 0 || slot >= SlotCount)
        {
            throw new ArgumentOutOfRangeException(nameof(slot));
        }

        return Path.Combine(_saveDirectory, $"Slot_{slot + 1}");
    }

    private void SetSlotDirectory(int slot)
    {
        string directory = GetSlotDirectory(slot);
        CurrentSlot = slot;
        _playStartTime = null;
        foreach (IUserData userData in UserDataList)
        {
            userData.SetSaveDirectory(directory);
        }
    }

    private void ClearCurrentSlot()
    {
        CurrentSlot = -1;
        _currentSlotMeta = null;
        _playStartTime = null;
        foreach (IUserData userData in UserDataList)
        {
            userData.SetSaveDirectory(null);
        }

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
        if (CurrentSlot < 0)
        {
            return false;
        }

        bool success = true;
        foreach (IUserData userData in UserDataList)
        {
            userData.SetDefaultData();
            success &= userData.LoadData();
        }

        OnChanged?.Invoke();
        return success;
    }

    public bool SaveAllUserData()
    {
        if (CurrentSlot < 0)
        {
            return false;
        }

        bool success = true;
        _isSavingAll = true;
        foreach (IUserData userData in UserDataList)
        {
            success &= userData.SaveData();
        }

        _isSavingAll = false;
        if (success)
        {
            success = SaveSlotMeta();
        }

        OnChanged?.Invoke();
        return success;
    }

    private void OnUserDataSaved()
    {
        if (!_isSavingAll)
        {
            SaveSlotMeta();
        }
    }

    private bool SaveSlotMeta()
    {
        AccumulatePlayTime();
        _currentSlotMeta.Currency = GetUserData<UserInventoryData>().Currency;
        return _slotMetaData.SaveData();
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
