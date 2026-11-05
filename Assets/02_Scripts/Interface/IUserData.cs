
using System;

public interface IUserData
{
    event Action OnSaved;
    void SetSaveDirectory(string saveDirectory);
    void SetDefaultData();
    bool LoadData();
    bool SaveData();
}
