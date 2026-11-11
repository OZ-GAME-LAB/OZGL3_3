using UnityEngine;

// 사망 후 부활 위치를 정하고 실제로 옮긴다.
// 현재 구간의 AreaText 데이터(RespawnMap, x, y, z)를 쓰고, 데이터가 없으면 PlayerSetting의 기본 위치로 부활한다.
public static class PlayerRespawn
{
    // 부활할 좌표와 맵을 찾는다. 찾지 못하면 position, destination은 null로 둔다.
    public static void Resolve(out Vector3? position, out MapRoot destination)
    {
        position = null;
        destination = null;

        if (GameManager.Instance == null)
        {
            return;
        }

        string regionId = GameManager.Area.CurrentRegionId;
        if (string.IsNullOrEmpty(regionId) == true)
        {
            Logger.LogWarning("PlayerRespawn: 현재 구간을 알 수 없어 기본 위치로 부활합니다.");
            return;
        }

        if (GameManager.DataTable.TryGetAreaSprite(regionId, out AreaTextData data) == false
            || string.IsNullOrEmpty(data.RespawnMap) == true)
        {
            Logger.LogWarning($"PlayerRespawn: '{regionId}'의 부활 데이터가 없어 기본 위치로 부활합니다.");
            return;
        }

        position = new Vector3(data.x, data.y, data.z);
        destination = FindMapRoot(data.RespawnMap, new Vector2(data.x, data.y));
    }

    // 맵을 바꿔야 하면 바꾸고, 플레이어를 부활시킨다.
    public static bool Execute(Vector3? position, MapRoot destination)
    {
        PlayerSetting playerSetting = PlayerSetting.Instance;
        if (playerSetting == null)
        {
            Logger.LogError("PlayerRespawn: PlayerSetting을 찾을 수 없습니다.");
            return false;
        }

        if (destination != null)
        {
            MapRoot current = null;
            if (WorldMapTracker.CurrentZone != null)
            {
                current = WorldMapTracker.CurrentZone.GetComponentInParent<MapRoot>(true);
            }

            if (current == null)
            {
                destination.gameObject.SetActive(true);
            }
            else if (current != destination)
            {
                current.DoSwitchMap(destination);
            }
        }

        return playerSetting.RevivePlayer(position);
    }

    // 구간 ID를 가진 방 중에서 부활 좌표를 포함하는 방의 MapRoot를 찾는다. 없으면 같은 구간의 첫 방을 쓴다.
    private static MapRoot FindMapRoot(string regionId, Vector2 position)
    {
        string target = regionId.Trim();
        MapRoot fallback = null;

        AreaZone[] zones = Object.FindObjectsByType<AreaZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < zones.Length; i++)
        {
            AreaZone zone = zones[i];
            if (string.IsNullOrEmpty(zone.RegionId) == true || zone.RegionId.Trim() != target)
            {
                continue;
            }

            MapRoot mapRoot = zone.GetComponentInParent<MapRoot>(true);
            if (mapRoot == null)
            {
                continue;
            }

            if (WorldMapTracker.TryGetWorldRect(zone, out Rect rect) == true && rect.Contains(position) == true)
            {
                return mapRoot;
            }

            if (fallback == null)
            {
                fallback = mapRoot;
            }
        }

        return fallback;
    }
}
