
using UnityEngine;

public class UnlockObject : MonoBehaviour, IInteractable
{
    private const string PLAYER_TAG = "Player";

    [Header("Unlock Settings")]
    [SerializeField] private string MapUnlockKey;
    [SerializeField] private Sprite AfterSprite;

    private Transform _transformPlayer;
    private PlayerInputHandler _playerInputHandler;

    private void Awake()
    {
        Collider2D Collider2D_UnlockObject = GetComponent<Collider2D>();
        Collider2D_UnlockObject.isTrigger = true;
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

        UIManager.Instance?.OpenInteractionPopupUI();
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
        if (_transformPlayer == null)
        {
            Logger.LogWarning(
                "UnlockObject: 상호작용 가능한 플레이어가 없습니다.");
            return;
        }

        Logger.Log("UnlockObject: Interact 호출");

        if(MapManager.Instance.IsMapUnlockKeyExist(MapUnlockKey))
        {
            return;
        }

        // 기능 구현 예정
        MapManager.Instance.UnlockMap(MapUnlockKey);
        GetComponent<SpriteRenderer>().sprite = AfterSprite;    
        GameManager.Sound.PlaySFX("Unlock");
    }
}
