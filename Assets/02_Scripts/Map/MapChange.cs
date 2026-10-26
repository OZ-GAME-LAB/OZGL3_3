using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MapChange : MonoBehaviour
{
    private const string STRING_PLAYER_TAG = "Player";

    [Header("Destination")]
    [SerializeField] private Transform Transform_DestinationPoint;

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
        if (!TryGetPlayerTransform(Collider2D_Other, out Transform Transform_Player))
        {
            return;
        }

        if (Transform_DestinationPoint == null)
        {
            return;
        }

        DoMovePlayer(Transform_Player);
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