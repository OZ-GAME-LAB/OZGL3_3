using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    public static DataTableManager DataTable { get { return Instance._dataTable; } }
    public static SoundManager Sound { get { return Instance._soundManager; } }


    #region Manager Variables

    private DataTableManager _dataTable = new();
    private SoundManager _soundManager = new();

    #endregion

    protected override void Init()
    {
        base.Init();

        _dataTable.LoadAllData();
        _soundManager.Init(gameObject);
    }

}
