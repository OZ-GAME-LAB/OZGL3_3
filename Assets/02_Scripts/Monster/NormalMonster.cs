using UnityEngine;

public class NormalMonster : MonsterBase
{
    [Header("Patrol Settings")]
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;
    private bool movingRight = true;

    protected override void Update()
    {
        base.Update();

        if (isAggro)
        {
            // 플레이어 추적 및 스킬 사용 시도
            TraceAndAttack();
        }
        else
        {
            // 평소 정찰
            Patrol();
        }
    }

    private void Patrol()
    {
        // 낭떠러지 및 벽 감지
        bool isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, 1f, groundLayer);
        bool isBlocked = Physics2D.Raycast(wallCheck.position, movingRight ? Vector2.right : Vector2.left, 0.5f, groundLayer);

        if (!isGrounded || isBlocked)
        {
            movingRight = !movingRight;
            transform.Rotate(0f, 180f, 0f); // 방향 전환
        }

        float dir = movingRight ? 1f : -1f;
        transform.Translate(Vector2.right * dir * moveSpeed * Time.deltaTime, Space.World);
    }

    private void TraceAndAttack()
    {
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // 사용 가능한 스킬이 있는지 확인
        for (int i = 0; i < skills.Count; i++)
        {
            if (distance <= skills[i].range && skillCooldownTimers[i] <= 0f)
            {
                // 스킬 발동
                skills[i].UseSkill(this, playerTransform);
                skillCooldownTimers[i] = skills[i].cooldown;
                return; // 한 프레임에 스킬 1개만 실행
            }
        }

        // 스킬 범위 밖이면 플레이어를 향해 이동
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        transform.Translate(new Vector2(direction.x, 0) * moveSpeed * Time.deltaTime, Space.World);
    }
}
