using System.Collections.Generic;
using UnityEngine;

// 플레이어가 어느 방(AreaZone) 안에 있는지 지켜보다가, 처음 들어간 방을 월드맵에 기록한다.
// 씬에 따로 배치하지 않아도 게임 시작 시 스스로 만들어진다.
public class WorldMapTracker : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const float CheckInterval = 0.25f;
    private const float RefreshInterval = 2f;

    private static WorldMapTracker _instance;

    private readonly List<AreaZone> _zones = new List<AreaZone>();
    private Transform _player;
    private float _checkTimer;
    private float _refreshTimer;

    // 가장 최근에 들어간 방의 키
    public static string CurrentRoomKey { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null)
        {
            return;
        }

        GameObject trackerObject = new GameObject("WorldMapTracker");
        DontDestroyOnLoad(trackerObject);
        _instance = trackerObject.AddComponent<WorldMapTracker>();
    }

    private void OnEnable()
    {
        MapChange.OnAfterMapChange += HandleAfterMapChange;
    }

    private void OnDisable()
    {
        MapChange.OnAfterMapChange -= HandleAfterMapChange;
    }

    private void HandleAfterMapChange()
    {
        // 맵이 바뀌면 방 목록과 위치를 바로 다시 확인한다.
        _refreshTimer = 0f;
        _checkTimer = 0f;
    }

    private void Update()
    {
        _refreshTimer -= Time.unscaledDeltaTime;
        if (_refreshTimer <= 0f)
        {
            RefreshZones();
            _refreshTimer = RefreshInterval;
        }

        _checkTimer -= Time.unscaledDeltaTime;
        if (_checkTimer > 0f)
        {
            return;
        }

        _checkTimer = CheckInterval;
        CheckCurrentRoom();
    }

    private void RefreshZones()
    {
        _zones.Clear();
        _zones.AddRange(FindObjectsByType<AreaZone>(FindObjectsInactive.Include, FindObjectsSortMode.None));
    }

    private void CheckCurrentRoom()
    {
        if (TryGetPlayerPosition(out Vector2 playerPosition) == false)
        {
            return;
        }

        AreaZone bestZone = null;
        float bestArea = float.MaxValue;

        for (int i = 0; i < _zones.Count; i++)
        {
            AreaZone zone = _zones[i];
            if (zone == null || zone.gameObject.activeInHierarchy == false)
            {
                continue;
            }

            if (TryGetWorldRect(zone, out Rect rect) == false || rect.Contains(playerPosition) == false)
            {
                continue;
            }

            // 방이 겹쳐 있으면 더 작은(안쪽) 방을 현재 방으로 본다.
            float area = rect.width * rect.height;
            if (area < bestArea)
            {
                bestArea = area;
                bestZone = zone;
            }
        }

        if (bestZone == null)
        {
            return;
        }

        CurrentRoomKey = GetRoomKey(bestZone);
        WorldMapData.MarkVisited(CurrentRoomKey);
    }

    public static bool TryGetPlayerPosition(out Vector2 position)
    {
        position = Vector2.zero;

        if (_instance == null)
        {
            return false;
        }

        if (_instance._player == null)
        {
            GameObject playerObject = GameObject.FindWithTag(PlayerTag);
            if (playerObject == null)
            {
                return false;
            }

            _instance._player = playerObject.transform;
        }

        position = _instance._player.position;
        return true;
    }

    // 방을 구분하는 키. ZoneId가 비어 있으면 하이어라키 경로를 대신 쓴다.
    public static string GetRoomKey(AreaZone zone)
    {
        if (string.IsNullOrEmpty(zone.ZoneId) == false)
        {
            return zone.ZoneId;
        }

        Transform current = zone.transform;
        string path = current.name;
        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }

    // 방(AreaZone의 콜라이더)이 차지하는 월드 사각형. 꺼져 있는 맵도 계산할 수 있다.
    public static bool TryGetWorldRect(AreaZone zone, out Rect rect)
    {
        rect = default;

        Collider2D collider = zone.GetComponent<Collider2D>();
        if (collider == null)
        {
            return false;
        }

        if (collider is BoxCollider2D box)
        {
            Transform zoneTransform = zone.transform;
            Vector2 half = box.size * 0.5f;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < 4; i++)
            {
                float x = ((i & 1) == 0) ? -half.x : half.x;
                float y = ((i & 2) == 0) ? -half.y : half.y;
                Vector3 world = zoneTransform.TransformPoint(box.offset + new Vector2(x, y));
                min = Vector2.Min(min, world);
                max = Vector2.Max(max, world);
            }

            rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            return true;
        }

        // 다른 모양의 콜라이더는 켜져 있을 때만 bounds를 쓸 수 있다.
        if (collider.enabled == true && collider.gameObject.activeInHierarchy == true)
        {
            Bounds bounds = collider.bounds;
            rect = Rect.MinMaxRect(bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y);
            return true;
        }

        return false;
    }
}
