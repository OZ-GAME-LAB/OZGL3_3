using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : SingletonBehaviour<GameManager>
{
    public static DataTableManager DataTable { get { return Instance._dataTable; } }
    public static SoundManager Sound { get { return Instance._soundManager; } }
    public static UserDataManager UserData { get { return Instance._userDataManager; } }
    public static NpcManager Npc { get { return Instance._npcManager; } }


    #region Manager Variables

    private DataTableManager _dataTable = new();
    private SoundManager _soundManager = new();
    private UserDataManager _userDataManager = new();
    private NpcManager _npcManager;

    #endregion

    protected override void Init()
    {
        base.Init();
        if (Instance != this)
        {
            return;
        }

        _dataTable.LoadAllData();
        _userDataManager.Init();
        _userDataManager.LoadAllUserData();
        _npcManager = new NpcManager(_dataTable, _userDataManager);
        _soundManager.Init(gameObject);
    }

    private void OnDestroy()
    {
        _npcManager?.Dispose();
    }

    private void Update()
    {
        if (!_npcManager.BlocksGameplayInput && UIManager.Instance != null)
        {
            string groupId = _npcManager.Dialogue.TakePendingGroup();
            if (!string.IsNullOrEmpty(groupId))
            {
                UIManager.Instance.OpenDialogueGroupUI(_npcManager, groupId);
            }
        }

        _npcManager.Dialogue.Tick(Time.unscaledDeltaTime);
        if (!_npcManager.IsOpen || _npcManager.OpenedFrame == Time.frameCount)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            if (_npcManager.CurrentType == NpcType.Story)
            {
                _npcManager.Dialogue.Skip();
            }
            else
            {
                _npcManager.Close();
            }

            return;
        }

        if (_npcManager.CurrentType == NpcType.Story && keyboard.anyKey.wasPressedThisFrame)
        {
            _npcManager.Dialogue.Advance();
        }
    }
}
