using UnityEngine;

public abstract class MonsterSkill : ScriptableObject
{
    protected string _skillName;
    public float CoolDown;
    public float Range;

    public abstract void UseSkill(MonsterBase caster, Transform target);
}

[CreateAssetMenu(menuName = "MonsterSkill/Projectile")]
public class ProjectileSkill : MonsterSkill
{
    [SerializeField] private GameObject _projectilePrefab;

    public override void UseSkill(MonsterBase caster, Transform target)
    {
        Vector2 dir = (target.position - caster.transform.position).normalized;
        GameObject proj = Instantiate(_projectilePrefab, caster.transform.position, Quaternion.identity);
        proj.GetComponent<Rigidbody2D>().linearVelocity = dir * 5f;
    }
}

[CreateAssetMenu(menuName = "MonsterSkill/SpawnAtPlayer")]
public class SpawnAtPlayerSkill : MonsterSkill
{
    [SerializeField] private GameObject _minionPrefab;

    public override void UseSkill(MonsterBase caster, Transform target)
    {
        Instantiate(_minionPrefab, target.position, Quaternion.identity);
    }
}

[CreateAssetMenu(menuName = "MonsterSkill/SpawnAtBoss")]
public class SpawnAtBossSkill : MonsterSkill
{
    [SerializeField] private GameObject _minionPrefab;
    private float _spawnOffset = 2f;

    public override void UseSkill(MonsterBase caster, Transform target)
    {
        Vector3 spawnPos = caster.transform.position + new Vector3(Random.Range(-_spawnOffset, _spawnOffset), 0, 0);
        Instantiate(_minionPrefab, spawnPos, Quaternion.identity);
    }
}
