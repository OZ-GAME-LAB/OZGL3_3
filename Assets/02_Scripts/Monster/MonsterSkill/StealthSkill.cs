using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class StealthSkill : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private MonsterBase _monsterBase;
    [SerializeField] private MonsterAnimatorController AnimController_NormalMonster;

    private void Awake()
    {
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (_monsterBase == null)
        {
            _monsterBase = GetComponent<MonsterBase>();
        }

        if (AnimController_NormalMonster == null)
        {
            AnimController_NormalMonster = GetComponent<MonsterAnimatorController>();
        }
    }

    public void OnEnterStealthSkillAnimation()
    {
        _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 0f);

        _monsterBase._isAttackUseable = false;
        _monsterBase._isUsingStealthSkill = true;

        StartCoroutine(CoStealthAttack());
    }

    private IEnumerator CoStealthAttack()
    {
        float randomAttackTime = UnityEngine.Random.Range(3f, 5f);
        yield return new WaitForSeconds(randomAttackTime);
        if (_monsterBase._playerTransform != null)
        {
            Vector3 targetPosition = _monsterBase._playerTransform.position;

            LayerMask groundLayer = LayerMask.GetMask("Ground");

            RaycastHit2D hit = Physics2D.Raycast(targetPosition, Vector2.down, 10f, groundLayer);

            if (hit.collider != null)
            {
                targetPosition = hit.point;
            }
            else
            {
                Vector3 safePosition = FindNearestGroundedPosition(targetPosition, groundLayer);
                if (safePosition != Vector3.zero)
                {
                    targetPosition = safePosition;
                }
                else
                {
                    targetPosition = transform.position;
                }
            }

            transform.position = targetPosition;
        }

        ExitStealth();
    }

    private Vector3 FindNearestGroundedPosition(Vector3 centerPos, LayerMask groundLayer)
    {
        float[] testOffsets = { -2f, 2f, -4f, 4f };

        foreach (float offset in testOffsets)
        {
            Vector3 checkPos = centerPos + new Vector3(offset, 2f, 0f);
            RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, 10f, groundLayer);

            if (hit.collider != null)
            {
                Vector3 targetTransform = hit.point + new Vector2(0, 1);
                return targetTransform;
            }
        }

        return Vector3.zero;
    }

    private void ExitStealth()
    {//[TODO] 나타나는 위치가 땅바닥이 없는 경우 떨어져서 죽는다
        _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 1f);
        AnimController_NormalMonster.ChangeState(MonsterState.Skill_2);
        _monsterBase._isAttackUseable = true;
        _monsterBase._isUsingStealthSkill = false;
    }
}
