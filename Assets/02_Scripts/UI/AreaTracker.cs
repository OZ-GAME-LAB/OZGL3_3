using System;
using System.Collections.Generic;

// 플레이어의 현재 존과 큰 구간을 기억하고, 바뀌면 알려 준다.
public class AreaTracker
{
    public string CurrentZoneId { get; private set; }
    public string CurrentRegionId { get; private set; }

    public event Action<string> OnZoneChanged;
    public event Action<string> OnRegionChanged;

    private readonly HashSet<string> _visitedRegions = new HashSet<string>();

    // 큰 구간이 바뀌었으면 true(이미지를 보여 줄 때). 처음 가는 구간인지도 알려 준다.
    public bool EnterZone(string zoneId, string regionId, out bool isFirstVisit)
    {
        isFirstVisit = false;

        if (CurrentZoneId != zoneId)
        {
            CurrentZoneId = zoneId;
            OnZoneChanged?.Invoke(zoneId);
        }

        if (CurrentRegionId == regionId)
        {
            return false;
        }

        CurrentRegionId = regionId;
        isFirstVisit = _visitedRegions.Add(regionId);
        OnRegionChanged?.Invoke(regionId);
        return true;
    }
}