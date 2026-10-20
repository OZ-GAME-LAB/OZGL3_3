
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapCollider2D))]
public class LadderZone : MonoBehaviour
{
    private const float Float_ExitTolerance = 0.02f;
    private const float Float_PlatformContactTolerance = 0.15f;

    private readonly List<Collider2D> _ignoredPlayerColliders =
        new List<Collider2D>();

    private Collider2D _playerColliderForBounds;
    private bool _isDropThroughActive;

    public Collider2D ExitPlatformCollider { get; private set; }

    public bool IsDropThroughActive
    {
        get { return _isDropThroughActive; }
    }

    private void Awake()
    {
        // 사다리 감지용 Trigger 설정
        TilemapCollider2D Collider2D_Ladder =
            GetComponent<TilemapCollider2D>();

        Collider2D_Ladder.isTrigger = true;

        // 자식 LadderExitPlatform의 Collider 탐색
        Transform Transform_LadderExitPlatform =
            transform.Find("LadderExitPlatform");

        if (Transform_LadderExitPlatform == null)
        {
            Logger.LogError(
                "LadderZone: 자식 LadderExitPlatform을 찾을 수 없습니다."
            );
            return;
        }

        ExitPlatformCollider =
            Transform_LadderExitPlatform.GetComponent<Collider2D>();

        if (ExitPlatformCollider == null)
        {
            Logger.LogError(
                "LadderZone: LadderExitPlatform에 Collider2D가 없습니다."
            );
        }
    }

    /// <summary>
    /// 플레이어가 출구 발판을 아래로 통과하도록 요청한다.
    /// </summary>
    public bool BeginDropThrough(Collider2D playerCollider)
    {
        if (_isDropThroughActive)
        {
            return false;
        }

        if (playerCollider == null || ExitPlatformCollider == null)
        {
            Logger.LogError(
                "LadderZone: 플레이어 또는 출구 발판 Collider가 없습니다."
            );
            return false;
        }

        Bounds PlayerBounds = playerCollider.bounds;
        Bounds PlatformBounds = ExitPlatformCollider.bounds;

        // 플레이어가 출구 발판 위에 있는지 확인
        bool IsAbovePlatform =
            PlayerBounds.min.y >=
            PlatformBounds.max.y - Float_PlatformContactTolerance;

        if (!IsAbovePlatform)
        {
            return false;
        }

        _ignoredPlayerColliders.Clear();
        _playerColliderForBounds = playerCollider;

        // Rigidbody2D가 연결된 플레이어를 기준으로 Collider 탐색
        Rigidbody2D Rigidbody2D_Player = playerCollider.attachedRigidbody;

        Transform Transform_PlayerRoot =
            Rigidbody2D_Player != null
                ? Rigidbody2D_Player.transform
                : playerCollider.transform.root;

        Collider2D[] Collider2D_PlayerChildren =
            Transform_PlayerRoot.GetComponentsInChildren<Collider2D>();

        for (int i = 0; i < Collider2D_PlayerChildren.Length; i++)
        {
            Collider2D Collider2D_Player = Collider2D_PlayerChildren[i];

            if (Collider2D_Player == null ||
                !Collider2D_Player.enabled ||
                Collider2D_Player.isTrigger ||
                Collider2D_Player == ExitPlatformCollider)
            {
                continue;
            }

            Physics2D.IgnoreCollision(
                Collider2D_Player,
                ExitPlatformCollider,
                true
            );

            _ignoredPlayerColliders.Add(Collider2D_Player);
        }

        if (_ignoredPlayerColliders.Count == 0)
        {
            _playerColliderForBounds = null;

            Logger.LogError(
                "LadderZone: 충돌을 무시할 플레이어 Collider가 없습니다."
            );
            return false;
        }

        _isDropThroughActive = true;
        return true;
    }

    private void Update()
    {
        CheckDropThroughCompletion();
    }

    /// <summary>
    /// 플레이어가 발판 아래로 완전히 내려갔는지 확인한다.
    /// </summary>
    private void CheckDropThroughCompletion()
    {
        if (!_isDropThroughActive)
        {
            return;
        }

        if (_playerColliderForBounds == null ||
            ExitPlatformCollider == null)
        {
            RestoreDropThroughCollision();
            return;
        }

        Bounds PlayerBounds = _playerColliderForBounds.bounds;
        Bounds PlatformBounds = ExitPlatformCollider.bounds;

        bool IsBelowPlatform =
            PlayerBounds.max.y <
            PlatformBounds.min.y - Float_ExitTolerance;

        if (IsBelowPlatform)
        {
            RestoreDropThroughCollision();
        }
    }

    /// <summary>
    /// 출구 발판과 플레이어의 충돌을 복구한다.
    /// </summary>
    private void RestoreDropThroughCollision()
    {
        for (int i = 0; i < _ignoredPlayerColliders.Count; i++)
        {
            Collider2D Collider2D_Player =
                _ignoredPlayerColliders[i];

            if (Collider2D_Player == null ||
                ExitPlatformCollider == null)
            {
                continue;
            }

            Physics2D.IgnoreCollision(
                Collider2D_Player,
                ExitPlatformCollider,
                false
            );
        }

        _ignoredPlayerColliders.Clear();
        _playerColliderForBounds = null;
        _isDropThroughActive = false;
    }

    private void OnDisable()
    {
        RestoreDropThroughCollision();
    }
}
