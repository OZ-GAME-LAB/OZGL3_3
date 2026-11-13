
using UnityEngine;

public class MapTest : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";

    private PlayerInputHandler _playerInputHandler;
    private PlayerController _playerController;

    private void OnEnable()
    {
        PlayerInputHandler.OnActiveInputChanged += BindPlayerInput;
        BindPlayerInput(PlayerInputHandler.ActiveInput);
    }

    private void OnDisable()
    {
        PlayerInputHandler.OnActiveInputChanged -= BindPlayerInput;
        BindPlayerInput(null);

        _playerController = null;
    }

    private void BindPlayerInput(PlayerInputHandler playerInputHandler)
    {
        if (_playerInputHandler == playerInputHandler)
        {
            return;
        }

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

    private void OnTriggerEnter2D(Collider2D collider2DOther)
    {
        if (!TryGetPlayerController(collider2DOther, out PlayerController playerController))
        {
            return;
        }

        _playerController = playerController;

        OpenInteractionUI();
    }

    private void OnTriggerExit2D(Collider2D collider2DOther)
    {
        if (!TryGetPlayerController(collider2DOther, out PlayerController playerController))
        {
            return;
        }

        if (_playerController != playerController)
        {
            return;
        }

        _playerController = null;

        CloseInteractionUI();
    }

    private void HandleInteractionRequested(Vector2 _)
    {
        if (_playerController == null)
        {
            return;
        }

        UnlockDoubleJump();
    }

    private void UnlockDoubleJump()
    {
        if (_playerController.IsDoubleJumpUnlocked)
        {
            Logger.Log("이미 더블 점프가 해금되어 있습니다.");
            return;
        }

        _playerController.SetDoubleJumpUnlocked(true);

        Logger.Log("Double Jump Unlocked");


        CloseInteractionUI();
    }

    private bool TryGetPlayerController(
        Collider2D collider2DOther,
        out PlayerController playerController)
    {
        playerController = null;

        if (collider2DOther == null)
        {
            return false;
        }

        Transform transformPlayerRoot =
            collider2DOther.attachedRigidbody != null
                ? collider2DOther.attachedRigidbody.transform
                : collider2DOther.transform;

        if (!collider2DOther.CompareTag(PLAYER_TAG) &&
            !transformPlayerRoot.CompareTag(PLAYER_TAG))
        {
            return false;
        }

        playerController =
            transformPlayerRoot.GetComponent<PlayerController>();

        if (playerController == null)
        {
            Logger.LogWarning("MapTest: PlayerController를 찾을 수 없습니다.");
            return false;
        }

        return true;
    }

    private void OpenInteractionUI()
    {
        UIManager.Instance?.OpenInteractionPopupUI();
    }

    private void CloseInteractionUI()
    {
        UIManager.Instance?.CloseInteractionPopupUI();
    }
}
