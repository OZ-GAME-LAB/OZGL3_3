using UnityEngine;

[CreateAssetMenu(menuName = "MonsterSkill/SpawnAtPlayer")]
public class SpawnAtPlayerSkill : MonsterSkill
{
    [SerializeField] private GameObject _minionPrefab;

    public override void UseSkill(MonsterBase caster, Transform target)
    {
        if (_minionPrefab == null)
        {
            Logger.LogError("[SpawnAtPlayerSkill] _minionPrefab이 Null입니다.");
            return;
        }
        Instantiate(_minionPrefab, target.position, Quaternion.identity);
    }
}
