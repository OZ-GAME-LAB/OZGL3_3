
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Collider2D))]
public class MapChange : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";

    private static bool _isTransitioning;

    public event Action OnBeforeMapChange;
    public event Action OnAfterMapChange;

    [Header("Portal Settings")]
    [SerializeField] private MapPortalType Enum_PortalType = MapPortalType.Contact;

    [Header("Destination")]
    [SerializeField] private Transform Transform_DestinationPoint;

    [Header("Fade Settings")]
    [SerializeField, Min(0f)] private float Float_FadeDuration = 0.5f;
    [SerializeField, Min(0f)] private float Float_PortalCooldown = 0.5f;

    private Transform _transformPlayer;

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

    private void OnTriggerEnter2D(Collider2D collider2DOther)
    {
        if (!TryGetPlayerTransform(
                collider2DOther,
                out Transform transformPlayer))
        {
            return;
        }

        _transformPlayer = transformPlayer;

        if (Enum_PortalType == MapPortalType.Contact)
        {
            DoStartMapChange(transformPlayer);
        }
    }

    private void OnTriggerExit2D(Collider2D collider2DOther)
    {
        if (!TryGetPlayerTransform(
                collider2DOther,
                out Transform transformPlayer))
        {
            return;
        }

        if (_transformPlayer == transformPlayer)
        {
            _transformPlayer = null;
        }
    }

    public void DoInteractMapChange()
    {
        if (Enum_PortalType != MapPortalType.Interaction ||
            _transformPlayer == null)
        {
            return;
        }

        DoStartMapChange(_transformPlayer);
    }

    private void DoStartMapChange(Transform transformPlayer)
    {
        if (_isTransitioning ||
            Transform_DestinationPoint == null)
        {
            return;
        }

        _isTransitioning = true;
        DoChangeMapAsync(transformPlayer).Forget();
    }

    private async UniTaskVoid DoChangeMapAsync(
        Transform transformPlayer)
    {
        bool isInputLocked = false;

        try
        {
            MapRoot mapRootCurrent =
                GetComponentInParent<MapRoot>(true);

            MapRoot mapRootDestination =
                Transform_DestinationPoint
                    .GetComponentInParent<MapRoot>(true);

            if (mapRootCurrent == null ||
                mapRootDestination == null)
            {
                Logger.LogWarning(
                    "MapChange: MapRoot를 찾을 수 없습니다.");
                return;
            }

            if (UIManager.Instance == null)
            {
                Logger.LogWarning(
                    "MapChange: UIManager를 찾을 수 없습니다.");
                return;
            }

            UIManager.Instance.OpenFadeUI();

            await UniTask.Delay(
                TimeSpan.FromSeconds(Float_FadeDuration));

            if (transformPlayer == null ||
                Transform_DestinationPoint == null)
            {
                return;
            }

            DoInvokeBeforeMapChange(ref isInputLocked);

            DoMovePlayer(transformPlayer);

            if (!mapRootCurrent.DoSwitchMap(mapRootDestination))
            {
                return;
            }

            DoInvokeAfterMapChange(ref isInputLocked);

            await UniTask.Delay(
                TimeSpan.FromSeconds(Float_PortalCooldown));
        }
        finally
        {
            DoInvokeAfterMapChange(ref isInputLocked);
            _isTransitioning = false;
        }
    }

    private void DoInvokeBeforeMapChange(ref bool isInputLocked)
    {
        isInputLocked = true;

        Logger.Log("MapChange: OnBeforeMapChange 호출");
        OnBeforeMapChange?.Invoke();
    }

    private void DoInvokeAfterMapChange(ref bool isInputLocked)
    {
        if (!isInputLocked)
        {
            return;
        }

        isInputLocked = false;

        Logger.Log("MapChange: OnAfterMapChange 호출");
        OnAfterMapChange?.Invoke();
    }

    private bool TryGetPlayerTransform(
        Collider2D collider2DOther,
        out Transform transformPlayer)
    {
        transformPlayer = null;

        if (collider2DOther == null)
        {
            return false;
        }

        Rigidbody2D rigidbody2DOther =
            collider2DOther.attachedRigidbody;

        Transform transformPlayerRoot =
            rigidbody2DOther != null
                ? rigidbody2DOther.transform
                : collider2DOther.transform;

        if (!collider2DOther.CompareTag(PLAYER_TAG) &&
            !transformPlayerRoot.CompareTag(PLAYER_TAG))
        {
            return false;
        }

        transformPlayer = transformPlayerRoot;
        return true;
    }

    private void DoMovePlayer(Transform transformPlayer)
    {
        Rigidbody2D rigidbody2DPlayer =
            transformPlayer.GetComponent<Rigidbody2D>();

        if (rigidbody2DPlayer != null)
        {
            rigidbody2DPlayer.position =
                Transform_DestinationPoint.position;

            rigidbody2DPlayer.linearVelocity = Vector2.zero;
        }
        else
        {
            transformPlayer.position =
                Transform_DestinationPoint.position;
        }
    }
}
