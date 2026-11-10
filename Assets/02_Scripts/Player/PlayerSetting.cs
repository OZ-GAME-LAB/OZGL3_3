using UnityEngine;

public class PlayerSetting : SingletonBehaviour<PlayerSetting>
{
    [SerializeField] private GameObject _playerPrefab;

    private static readonly Vector3 DefaultSpawnPosition =
    new Vector3(21f, -27f, 0f);

    private GameObject _playerInstance;
    private PlayerInputHandler _playerInputHandler;
    private CameraFollowPlayer _cameraFollowPlayer;

    public void Awake()
    {
        base.Init();
        if (Instance != this)
        {
            return;
        }

        _playerPrefab = Resources.Load<GameObject>("Player/Player");
    }

    public bool PreparePlayer(Vector3? spawnPosition = null)
    {
        if (_playerPrefab == null)
        {
            _playerPrefab = Resources.Load<GameObject>("Player/Player");
        }

        if (!TryGetCameraFollowPlayer())
        {
            Logger.LogError("PlayerSetting: CameraFollowPlayer가 없습니다.");
            return false;
        }

        bool createdPlayer = false;
        if (_playerInstance == null)
        {
            Vector3 position = spawnPosition ?? DefaultSpawnPosition;
            _playerInstance = Instantiate(
                _playerPrefab,
                position,
                Quaternion.identity);
            createdPlayer = true;
        }
        else if (spawnPosition.HasValue)
        {
            MovePlayer(spawnPosition);
        }

        _playerInputHandler =
            _playerInstance.GetComponent<PlayerInputHandler>();

        if (_playerInputHandler == null)
        {
            Logger.LogError(
                "PlayerSetting: Player 프리팹에서 PlayerInputHandler를 찾을 수 없습니다.");
            Destroy(_playerInstance);
            _playerInstance = null;
            return false;
        }

        if (createdPlayer)
        {
            if (GameManager.Instance == null || GameManager.UserData == null)
            {
                Logger.LogError(
                    "PlayerSetting: UserDataManager가 준비되지 않았습니다.");
                Destroy(_playerInstance);
                _playerInstance = null;
                _playerInputHandler = null;
                return false;
            }

            PlayerArtifactEquipment artifactEquipment =
                _playerInstance.GetComponent<PlayerArtifactEquipment>();
            UserArtifactData artifactData =
                GameManager.UserData.GetUserData<UserArtifactData>();

            if (artifactEquipment == null ||
                !artifactEquipment.LoadFromSave(artifactData))
            {
                Logger.LogError(
                    "PlayerSetting: 저장된 아티팩트를 플레이어에 적용하지 못했습니다.");
                Destroy(_playerInstance);
                _playerInstance = null;
                _playerInputHandler = null;
                return false;
            }
        }

        _playerInputHandler.ClearInput();
        _playerInputHandler.enabled = false;
        _cameraFollowPlayer.SetTarget(_playerInstance.transform);
        return true;
    }

    public bool MovePlayer(Vector3? destination = null)
    {
        if (_playerPrefab == null)
        {
            _playerPrefab = Resources.Load<GameObject>("Player/Player");
        }

        if (_playerInstance == null)
        {
            Logger.LogError("PlayerSetting: 이동할 플레이어가 없습니다.");
            return false;
        }

        if (!TryGetCameraFollowPlayer())
        {
            Logger.LogError("PlayerSetting: CameraFollowPlayer가 없습니다.");
            return false;
        }

        Vector3 position = destination ?? DefaultSpawnPosition;
        Rigidbody2D rigidbody2D = _playerInstance.GetComponent<Rigidbody2D>();
        if (rigidbody2D != null)
        {
            rigidbody2D.position = position;
            rigidbody2D.linearVelocity = Vector2.zero;

            Vector3 playerPosition = _playerInstance.transform.position;
            playerPosition.z = position.z;
            _playerInstance.transform.position = playerPosition;
        }
        else
        {
            _playerInstance.transform.position = position;
        }

        _cameraFollowPlayer.SetTarget(_playerInstance.transform);
        return true;
    }

    private bool TryGetCameraFollowPlayer()
    {
        if (_playerPrefab == null)
        {
            _playerPrefab = Resources.Load<GameObject>("Player/Player");
        }

        if (_cameraFollowPlayer != null)
        {
            return true;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return false;
        }

        _cameraFollowPlayer =
            mainCamera.GetComponent<CameraFollowPlayer>();

        return _cameraFollowPlayer != null;
    }

    public bool StartPreparedPlayer()
    {
        if (_playerPrefab == null)
        {
            _playerPrefab = Resources.Load<GameObject>("Player/Player");
        }

        if (_playerInstance == null || _playerInputHandler == null)
        {
            Logger.LogError("PlayerSetting: 시작 전에 플레이어를 준비해야 합니다.");
            return false;
        }

        _playerInputHandler.ClearInput();
        _playerInputHandler.enabled = true;
        return true;
    }
}
