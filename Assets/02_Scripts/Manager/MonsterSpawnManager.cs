using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject Prefab_Monster;
    [SerializeField] private Transform Root_Monster;

    public static MonsterSpawnManager Instance { get; set; }

    private int _objectInstanceKeyGenerator = 0;

    private Dictionary<int, GameObject> _createdMonsterContainer = new Dictionary<int, GameObject>();


    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            TestInstantiateMonster();
        }
    }

    private void TestInstantiateMonster()
    {
        RequestSpawnEnemy();
    }

    public void RequestSpawnEnemy()
    {
        if (Prefab_Monster == null)
        {
            Debug.LogWarning("프리팹이 등록되지 않은 몬스터 입니다.");
            return;
        }

        var gObj = Instantiate(Prefab_Monster, Root_Monster);
        gObj.SetActive(false);
        if (gObj == null)
        {
            Debug.LogWarning("생성에 실패한 게임 오브젝트 입니다.");
            return;
        }

        _objectInstanceKeyGenerator++;

        if (_createdMonsterContainer.ContainsKey(_objectInstanceKeyGenerator) == true)
        {
            Debug.LogWarning("이미 동일한 키가 발급된 게임 오브젝트가 존재합니다");
            return;
        }

        _createdMonsterContainer.Add(_objectInstanceKeyGenerator, gObj);
        InitGeneratedEntityObject(_objectInstanceKeyGenerator, gObj);

        Debug.Log($"키: {_objectInstanceKeyGenerator}의 객체 {gObj.name}이 호출되었습니다.");
    }

    private void InitGeneratedEntityObject(int generatedId, GameObject gObj)
    {
        NormalMonster monster = gObj.GetComponent<NormalMonster>();
        if (monster == null)
        {
            Debug.LogWarning($"생성된 {gObj.name}의 InstanceId를 대입할 수 있는 컴포넌트를 가져올 수 없습니다!");
            return;
        }

        //monster.InitEnemyInfo(generatedId);
    }


    public GameObject GetEntityObjectCanBeNull(int instanceId)
    {
        if (_createdMonsterContainer.ContainsKey(instanceId) == false)
        {
            Debug.LogWarning($"{instanceId}는 존재하지 않습니다.");
            return null;
        }

        return _createdMonsterContainer[instanceId];
    }

    public void RequestDestroyEntityObject(int instanceId)
    {
        var gObj = GetEntityObjectCanBeNull(instanceId);
        if (gObj == null)
        {
            return;
        }

        _createdMonsterContainer.Remove(instanceId);
        Destroy(gObj);
    }
}
