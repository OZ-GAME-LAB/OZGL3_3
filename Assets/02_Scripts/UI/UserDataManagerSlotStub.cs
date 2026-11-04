// 임시 스텁: 3개 슬롯 모두 같은 단일 세이브(Assets/UserData)를 보도록 한 테스트용.
// UserDataManager에 실제 메서드가 생기면 이 파일을 삭제한다.
using System;
using System.IO;
using UnityEngine;

public static class UserDataManagerSlotStub
{
    private static readonly string[] _fileNames =
    {
        "ArtifactData.json", "ShopData.json", "DialogueData.json", "InventoryData.json"
    };

    private static string Directory_ => Path.Combine(Application.dataPath, "UserData");

    public static bool HasSave(this UserDataManager manager, int slot)
    {
        foreach (string fileName in _fileNames)
        {
            if (File.Exists(Path.Combine(Directory_, fileName))) { return true; }
        }
        return false;
    }

    public static DateTime? GetSaveTime(this UserDataManager manager, int slot)
    {
        DateTime? latest = null;
        foreach (string fileName in _fileNames)
        {
            string path = Path.Combine(Directory_, fileName);
            if (File.Exists(path) == false) { continue; }

            DateTime time = File.GetLastWriteTime(path);
            if (latest == null || time > latest) { latest = time; }
        }
        return latest;
    }

    public static void SelectSlot(this UserDataManager manager, int slot) { }

    public static bool DeleteSlot(this UserDataManager manager, int slot)
    {
        foreach (string fileName in _fileNames)
        {
            string path = Path.Combine(Directory_, fileName);
            if (File.Exists(path)) { File.Delete(path); }
            if (File.Exists(path + ".meta")) { File.Delete(path + ".meta"); }
        }
        return true;
    }
}