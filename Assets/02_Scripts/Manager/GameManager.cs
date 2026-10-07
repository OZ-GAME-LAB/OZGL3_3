using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    public static DataTableManager DataTable { get { return Instance._dataTable; } }


    #region Manager Variables

    private DataTableManager _dataTable = new();

    #endregion

    protected override void Init()
    {
        base.Init();

        _dataTable.LoadAllData();
    }

}
