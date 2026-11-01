using UnityEngine;

// 존(맵 하나)을 대표한다. 소속된 큰 구간 ID를 가지고, 플레이어가 들어오면 현재 위치를 갱신하고 구간 이미지를 보여 준다.
public class AreaZone : MonoBehaviour
{
    [SerializeField] private string _zoneId;      // 이 맵의 ID (예: Zone_ForestVillage). 지금은 비워 둬도 됨
    [SerializeField] private string _regionId;    // 큰 구간 ID, AreaTextTable의 Id (예: Area_AstraForest)
    [SerializeField] private bool _isShowOnce = true;

    public string ZoneId => _zoneId;
    public string RegionId => _regionId;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") == false)
        {
            return;
        }

        if (string.IsNullOrEmpty(_regionId) == true)
        {
            Logger.LogError($"AreaZone: {name}의 _regionId가 비어 있습니다.");
            return;
        }

        // 큰 구간이 바뀐 경우에만 이미지를 보여 준다.
        if (GameManager.Area.EnterZone(_zoneId, _regionId, out bool isFirstVisit) == false)
        {
            return;
        }

        if (_isShowOnce == true && isFirstVisit == false)
        {
            return;
        }

        // 타이틀 화면이 떠 있는 동안에는 보여 주지 않는다. (시작 버튼에서 보여 줌)
        if (UIManager.Instance.IsOpenUI(UIType.GameStartUI) == true)
        {
            return;
        }

        UIManager.Instance.OpenAreaSpriteUI(_regionId);
    }
}