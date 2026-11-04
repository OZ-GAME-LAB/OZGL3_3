using UnityEngine;

[CreateAssetMenu(menuName = "MonsterSkill/Projectile")]
public class ProjectileSkill : MonsterSkill
{
    [SerializeField] private GameObject _projectilePrefab;


    public override void UseSkill(MonsterBase caster, Transform target)
    {
        if (_projectilePrefab == null)
        {
            Logger.LogError("[ProjectileSkill] _projectilePrefab이 Null입니다.");
            return;
        }
        Vector2 dir = (target.position - caster.transform.position).normalized;
        GameObject proj = Instantiate(_projectilePrefab, caster.transform.position, Quaternion.identity);

        var spriteRenderer = proj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            bool isTargetRight = target.position.x > caster.transform.position.x;

            spriteRenderer.flipX = isTargetRight;
        }

        if (caster._monsterType == MonsterType.Ground)
        {
            proj.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(dir.x * 10f, 0f);
        }
        else
        {
            proj.GetComponent<Rigidbody2D>().linearVelocity = dir * 5f;
        }
    }
}
