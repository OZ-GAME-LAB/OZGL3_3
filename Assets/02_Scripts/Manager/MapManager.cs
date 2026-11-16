
using System.Collections.Generic;
using UnityEngine;

public class MapManager : SingletonBehaviour<MapManager>
{
    // 맵 언락 키 목록
    private readonly HashSet<string> _mapUnlockKeys = new HashSet<string>();

    public void UnlockMap(string String_MapUnlockKey)
    {
        if (string.IsNullOrWhiteSpace(String_MapUnlockKey))
        {
            Logger.LogError("MapManager: 맵 언락 키가 비어 있습니다.");
            return;
        }

        if (!IsMapUnlockKeyExist(String_MapUnlockKey))
        {
            _mapUnlockKeys.Add(String_MapUnlockKey);

            Logger.Log($"MapManager: 맵 언락 성공 - {String_MapUnlockKey}");
            return;
        }
    }

    public bool IsMapUnlockKeyExist(string String_MapUnlockKey)
    {
        if (string.IsNullOrWhiteSpace(String_MapUnlockKey))
        {
            return false;
        }

        return _mapUnlockKeys.Contains(String_MapUnlockKey);
    }

    public bool IsMapUnlocked(string String_MapUnlockKey)
    {
        return IsMapUnlockKeyExist(String_MapUnlockKey);
    }
}
