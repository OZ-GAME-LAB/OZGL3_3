using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Collider2D))]
public class MapChange : MonoBehaviour
{
    private const string STRING_PLAYER_TAG = "Player";

    private static bool _isTransitioning;

    [Header("Destination")]
    [SerializeField] private Transform Transform_DestinationPoint;

    [Header("Fade Settings")]
    [SerializeField, Min(0f)] private float Float_FadeDuration = 0.5f;
    [SerializeField, Min(0f)] private float Float_TransitionCooldown = 0.5f;

    private void Awake()
    {
        Collider2D Collider2D_MapChange = GetComponent<Collider2D>();
        Collider2D_MapChange.isTrigger = true;

        if (Transform_DestinationPoint == null)
        {
            Logger.LogWarning(
                "MapChange: 목적지 위치 오브젝트를 Inspector에 연결하세요.");
        }
    }

    private void OnTriggerEnter2D(Collider2D Collider2D_Other)
    {
        if (_isTransitioning)
        {
            return;
        }

        if (!TryGetPlayerTransform(
                Collider2D_Other,
                out Transform Transform_Player))
        {
            return;
        }

        if (Transform_DestinationPoint == null)
        {
            return;
        }

        _isTransitioning = true;
        DoChangeMapAsync(Transform_Player).Forget();
    }

    private async UniTaskVoid DoChangeMapAsync(
        Transform Transform_Player)
    {
        try
        {
            // 현재 맵과 목적지 맵 검색
            MapRoot MapRoot_Current =
                GetComponentInParent<MapRoot>(true);

            MapRoot MapRoot_Destination =
                Transform_DestinationPoint
                    .GetComponentInParent<MapRoot>(true);

            if (MapRoot_Current == null ||
                MapRoot_Destination == null)
            {
                Logger.LogWarning(
                    "MapChange: 현재 맵 또는 목적지 맵의 MapRoot를 찾을 수 없습니다.");

                return;
            }

            // 페이드 UI 실행
            if (UIManager.Instance == null)
            {
                Logger.LogWarning(
                    "MapChange: UIManager를 찾을 수 없습니다.");

                return;
            }

            UIManager.Instance.OpenFadeUI();

            // 페이드 애니메이션 시간만큼 대기
            await UniTask.Delay(
                TimeSpan.FromSeconds(Float_FadeDuration));

            if (Transform_Player == null ||
                Transform_DestinationPoint == null)
            {
                return;
            }

            // 목적지 위치로 플레이어 이동
            DoMovePlayer(Transform_Player);

            // MapRoot에 맵 전환 요청
            if (!MapRoot_Current.DoSwitchMap(MapRoot_Destination))
            {
                return;
            }

            // 연속 맵 전환 방지
            await UniTask.Delay(
                TimeSpan.FromSeconds(Float_TransitionCooldown));
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    private bool TryGetPlayerTransform(
        Collider2D Collider2D_Other,
        out Transform Transform_Player)
    {
        Transform_Player = null;

        if (Collider2D_Other == null)
        {
            return false;
        }

        Rigidbody2D Rigidbody2D_Other =
            Collider2D_Other.attachedRigidbody;

        Transform Transform_PlayerRoot =
            Rigidbody2D_Other != null
                ? Rigidbody2D_Other.transform
                : Collider2D_Other.transform;

        if (!Collider2D_Other.CompareTag(STRING_PLAYER_TAG) &&
            !Transform_PlayerRoot.CompareTag(STRING_PLAYER_TAG))
        {
            return false;
        }

        Transform_Player = Transform_PlayerRoot;

        return true;
    }

    private void DoMovePlayer(Transform Transform_Player)
    {
        Rigidbody2D Rigidbody2D_Player =
            Transform_Player.GetComponent<Rigidbody2D>();

        if (Rigidbody2D_Player != null)
        {
            Rigidbody2D_Player.position =
                Transform_DestinationPoint.position;

            Rigidbody2D_Player.linearVelocity = Vector2.zero;
        }
        else
        {
            Transform_Player.position =
                Transform_DestinationPoint.position;
        }
    }
}