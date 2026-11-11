using System;
using System.Collections.Generic;

// 월드맵에서 가 본 방을 기억한다.
// 세이브 연동 전까지는 게임을 켜 둔 동안만 유지된다. (저장하려면 GetVisitedKeys / SetVisitedKeys를 세이브 데이터에 연결)
public static class WorldMapData
{
    private static readonly HashSet<string> _visitedRoomKeys = new HashSet<string>();

    // 지도 아이템처럼 안 가 본 방의 윤곽을 미리 보여 주고 싶을 때 true로 바꾼다.
    public static bool IsRevealAll { get; set; }

    public static event Action OnChanged;

    public static bool IsVisited(string roomKey)
    {
        return _visitedRoomKeys.Contains(roomKey);
    }

    // 처음 방문한 경우에만 true
    public static bool MarkVisited(string roomKey)
    {
        if (string.IsNullOrEmpty(roomKey) == true)
        {
            return false;
        }

        if (_visitedRoomKeys.Add(roomKey) == false)
        {
            return false;
        }

        OnChanged?.Invoke();
        return true;
    }

    public static void Clear()
    {
        _visitedRoomKeys.Clear();
        OnChanged?.Invoke();
    }

    public static string[] GetVisitedKeys()
    {
        string[] keys = new string[_visitedRoomKeys.Count];
        _visitedRoomKeys.CopyTo(keys);
        return keys;
    }

    public static void SetVisitedKeys(IEnumerable<string> roomKeys)
    {
        _visitedRoomKeys.Clear();

        if (roomKeys != null)
        {
            foreach (string key in roomKeys)
            {
                if (string.IsNullOrEmpty(key) == false)
                {
                    _visitedRoomKeys.Add(key);
                }
            }
        }

        OnChanged?.Invoke();
    }
}
