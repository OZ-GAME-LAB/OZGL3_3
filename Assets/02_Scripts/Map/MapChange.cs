
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Collider2D))]
public class MapChange : MonoBehaviour, IInteractable
{
    private const string PLAYER_TAG = "Player";

    private static bool _isTransitioning;

    public static event Action OnBeforeMapChange;
    public static event Action OnAfterMapChange;

    [Header("Portal Settings")]
    [SerializeField] private MapPortalType Enum_PortalType = MapPortalType.Contact;
    [SerializeField] private String UnlockKey;

    [Header("Destination")]
    [SerializeField] private Transform Transform_DestinationPoint;

    [Header("Fade Settings")]
    [SerializeField, Min(0f)] private float Float_FadeDuration = 0.5f;
    [SerializeField, Min(0f)] private float Float_PortalCooldown = 0.5f;

    private Transform _transformPlayer;
    private PlayerInputHandler _playerInputHandler;

    public MapPortalType CurrentPortalType => Enum_PortalType;

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

    private void OnEnable()
    {
        PlayerInputHandler.OnActiveInputChanged += BindPlayerInput;
        BindPlayerInput(PlayerInputHandler.ActiveInput);
    }

    private void OnDisable()
    {
        PlayerInputHandler.OnActiveInputChanged -= BindPlayerInput;
        BindPlayerInput(null);
        _transformPlayer = null;
    }

    private void BindPlayerInput(PlayerInputHandler playerInputHandler)
    {
        if (_playerInputHandler != null)
        {
            _playerInputHandler.OnInteractionRequested -= HandleInteractionRequested;
        }

        _playerInputHandler = playerInputHandler;

        if (_playerInputHandler != null)
        {
            _playerInputHandler.OnInteractionRequested += HandleInteractionRequested;
        }
    }

     private void HandleInteractionRequested(Vector2 playerPosition)
    {
        if (CurrentPortalType != MapPortalType.Interaction)
        {
            return;
        }

        if (_transformPlayer == null)
        {
            return;
        }

        if (GameManager.Instance == null ||
            GameManager.Npc.BlocksGameplayInput)
        {
            return;
        }

        Interact();
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

        if (CurrentPortalType == MapPortalType.Contact)
        {
            DoStartMapChange(transformPlayer);
        }
        else 
        {
            UIManager.Instance?.OpenInteractionPopupUI();
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

        UIManager.Instance?.CloseInteractionPopupUI();

        if (_transformPlayer == transformPlayer)
        {
            _transformPlayer = null;
        }

    }

    private void DoStartMapChange(Transform transformPlayer)
    {
        bool UnlockKeyIsValid = string.IsNullOrEmpty(UnlockKey) || MapManager.Instance.IsMapUnlocked(UnlockKey);
        if (_isTransitioning ||
            Transform_DestinationPoint == null ||
            !UnlockKeyIsValid)
        {
            return;
        }
        Logger.Log($"MapChange: {UnlockKeyIsValid}");
        _isTransitioning = true;
        DoChangeMapAsync(transformPlayer).Forget();
    }

    private async UniTaskVoid DoChangeMapAsync(
        Transform transformPlayer)
    {
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

            Logger.Log("MapChange: OnBeforeMapChange 호출");
            OnBeforeMapChange?.Invoke();

            UIManager.Instance.OpenFadeUI();

            await UniTask.Delay(
                TimeSpan.FromSeconds(Float_FadeDuration));

            if (transformPlayer == null ||
                Transform_DestinationPoint == null)
            {
                return;
            }

            if (PlayerSetting.Instance == null ||
                !PlayerSetting.Instance.MovePlayer(
                    Transform_DestinationPoint.position))
            {
                Logger.LogError(
                    "MapChange: 플레이어 이동 실패");
                return;
            }

            if (!mapRootCurrent.DoSwitchMap(mapRootDestination))
            {
                return;
            }

            Logger.Log("MapChange: OnAfterMapChange 호출");
            OnAfterMapChange?.Invoke();

            await UniTask.Delay(
                TimeSpan.FromSeconds(Float_PortalCooldown));
        }
        finally
        {
            _isTransitioning = false;
        }
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

    public void Interact()
    {
        if (CurrentPortalType != MapPortalType.Interaction)
        {
            return;
        }

        if (_transformPlayer == null)
        {
            Logger.LogWarning(
                "MapChange: 상호작용 가능한 플레이어가 없습니다.");
            return;
        }

        Logger.Log("MapChange: Interact 호출");

        DoStartMapChange(_transformPlayer);
    }
}
