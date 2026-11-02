using UnityEngine;

[CreateAssetMenu(menuName = "MonsterSkill/Projectile")]
public class ProjectileSkill : MonsterSkill
{
    [SerializeField] private GameObject _projectilePrefab;

    public override void UseSkill(MonsterBase caster, Transform target)
    {//[TODO] 지상 몬스터인 경우 y값은 빼고 일자로만 날아가도록 수정
        if (_projectilePrefab == null)
        {
            Logger.LogError("[ProjectileSkill] _projectilePrefab이 Null입니다.");
            return;
        }
        Vector2 dir = (target.position - caster.transform.position).normalized;
        GameObject proj = Instantiate(_projectilePrefab, caster.transform.position, Quaternion.identity);
        proj.GetComponent<Rigidbody2D>().linearVelocity = dir * 5f;
    }
}
