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

    private Collider2D _collider2D_MapChange;

    private void Awake()
    {
        _collider2D_MapChange = GetComponent<Collider2D>();
        _collider2D_MapChange.isTrigger = true;

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
            // 페이드 애니메이션 시작
            UIManager.Instance.OpenFadeUI();

            // 설정한 시간만큼 대기
            await UniTask.Delay(
                TimeSpan.FromSeconds(Float_FadeDuration));

            if (Transform_Player == null ||
                Transform_DestinationPoint == null)
            {
                return;
            }

            // 대기 완료 후 플레이어 이동
            DoMovePlayer(Transform_Player);

            // 도착 직후 다른 맵 전환 트리거가 실행되는 현상 방지
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