using UnityEngine;

[CreateAssetMenu(menuName = "MonsterSkill/SpawnAtBoss")]
public class SpawnAtBossSkill : MonsterSkill
{
    [SerializeField] private GameObject _minionPrefab;
    [SerializeField] private int _spawnAmount = 4;
    private float _spawnOffset = 2f;

    public override void UseSkill(MonsterBase caster, Transform target)
    {
        if (_minionPrefab == null)
        {
            Logger.LogError("[SpawnAtBossSkill] _minionPrefab이 Null입니다.");
            return;
        }

        int spawnAmount = _spawnAmount;
        while (spawnAmount > 0)
        {
            Vector3 spawnPos = caster.transform.position + new Vector3(Random.Range(-_spawnOffset, _spawnOffset), 0, 0);
            Instantiate(_minionPrefab, spawnPos, Quaternion.identity);
            spawnAmount--;
        }
    }
}