using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    public static DataTableManager DataTable { get { return Instance._dataTable; } }
    public static SoundManager Sound { get { return Instance._soundManager; } }
    public static UserDataManager UserData { get { return Instance._userDataManager; } }
    public static NpcManager Npc { get { return Instance._npcManager; } }

    public static AreaTracker Area { get { return Instance._areaTracker; } }
    private AreaTracker _areaTracker = new AreaTracker();

    #region Manager Variables

    private DataTableManager _dataTable = new();
    private SoundManager _soundManager = new();
    private UserDataManager _userDataManager = new();
    private NpcManager _npcManager;
    private NpcController _npcController;
    private bool _resumePlaySession;

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
        _npcManager = new NpcManager(_dataTable, _userDataManager);
        _npcController = gameObject.AddComponent<NpcController>();
        _npcController.Init(_npcManager);
        _soundManager.Init(gameObject);
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            _resumePlaySession = _userDataManager.IsPlaySessionActive;
            _userDataManager.EndPlaySession();
            if (_userDataManager.CurrentSlot >= 0)
            {
                _userDataManager.SaveAllUserData();
            }
        }
        else if (_resumePlaySession)
        {
            _userDataManager.BeginPlaySession();
            _resumePlaySession = false;
        }
    }

    private void OnApplicationQuit()
    {
        _userDataManager.EndPlaySession();
        if (_userDataManager.CurrentSlot >= 0)
        {
            _userDataManager.SaveAllUserData();
        }
    }

    private void OnDestroy()
    {
        _npcController?.Dispose();
        _npcManager?.Dispose();
    }
}
