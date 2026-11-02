using UnityEngine;

public class MapRoot : MonoBehaviour
{
    public bool DoSwitchMap(MapRoot MapRoot_Destination)
    {
        if (MapRoot_Destination == null)
        {
            Logger.LogWarning(
                "MapRoot: 목적지 MapRoot를 찾을 수 없습니다.");

            return false;
        }

        // 현재 맵과 목적지 맵이 같은 경우
        if (MapRoot_Destination == this)
        {
            return true;
        }

        // 목적지 맵 활성화
        MapRoot_Destination.gameObject.SetActive(true);

        // 현재 맵 비활성화
        gameObject.SetActive(false);

        return true;
    }
}