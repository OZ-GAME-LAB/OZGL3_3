using UnityEngine;

public class NpcInteraction : MonoBehaviour
{
    private const float _interactionRange = 1.6f;

    [SerializeField] private NpcType _npcType;
    [SerializeField] private string _npcId;
    [SerializeField] private Sprite _portrait;
    [SerializeField] private AudioSource _feedbackSource;
    [SerializeField] private AudioClip _equipRejectedClip;

    private PlayerInputHandler _playerInputHandler;
    private ArtifactService _artifacts;

    private void OnEnable()
    {
        if (_playerInputHandler != null)
        {
            _playerInputHandler.OnInteractionRequested += HandleInteractionRequested;
        }
    }

    private void OnDisable()
    {
        if (_playerInputHandler != null)
        {
            _playerInputHandler.OnInteractionRequested -= HandleInteractionRequested;
        }
    }

    private void Start()
    {
        if (PlayerStatSet.Instance != null)
        {
            _playerInputHandler = PlayerStatSet.Instance.GetComponent<PlayerInputHandler>();
            _playerInputHandler.OnInteractionRequested += HandleInteractionRequested;
        }

        if (_npcType != NpcType.Artifact)
        {
            return;
        }

        _artifacts = GameManager.Npc.Artifacts;
        _artifacts.OnEquipRejected += HandleEquipRejected;
    }

    private void OnDestroy()
    {
        if (_artifacts != null)
        {
            _artifacts.OnEquipRejected -= HandleEquipRejected;
        }
    }

    private void HandleInteractionRequested(Vector2 playerPosition)
    {
        if (GameManager.Instance == null || GameManager.Npc.BlocksGameplayInput)
        {
            return;
        }

        float distance = ((Vector2)transform.position - playerPosition).sqrMagnitude;
        if (distance > _interactionRange * _interactionRange)
        {
            return;
        }

        Interact();
    }

    public void Interact()
    {
        TryInteract(GameManager.Npc);
    }

    public void Configure(NpcType npcType, string npcId, Sprite portrait)
    {
        _npcType = npcType;
        _npcId = npcId;
        _portrait = portrait;
    }

    public bool TryInteract(NpcManager npcManager)
    {
        UIManager uiManager = UIManager.Instance;
        if (uiManager == null)
        {
            Logger.LogError("NPC UI를 열려면 UIManager가 필요합니다.");
            return false;
        }

        return _npcType switch
        {
            NpcType.Shop => uiManager.OpenShopUI(npcManager, _npcId, _portrait),
            NpcType.Artifact => uiManager.OpenArtifactUI(npcManager, _npcId, _portrait),
            NpcType.Story => uiManager.OpenDialoaguetUI(npcManager, _npcId, _portrait),
            _ => false
        };
    }

    private void HandleEquipRejected()
    {
        if (!isActiveAndEnabled || GameManager.Npc.CurrentType != NpcType.Artifact || GameManager.Npc.CurrentNpcId != _npcId)
        {
            return;
        }

        if (_feedbackSource != null && _equipRejectedClip != null)
        {
            _feedbackSource.PlayOneShot(_equipRejectedClip);
        }
    }
}
