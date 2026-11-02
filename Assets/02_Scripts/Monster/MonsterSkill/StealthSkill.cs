using System;
using System.Collections;
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
            transform.position = _monsterBase._playerTransform.position;
        }

        ExitStealth();
    }

    private void ExitStealth()
    {//[TODO] 나타나는 위치가 땅바닥이 없는 경우 떨어져서 죽는다
        _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 1f);
        AnimController_NormalMonster.ChangeState(MonsterState.Skill_2);
        _monsterBase._isAttackUseable = true;
        _monsterBase._isUsingStealthSkill = false;
    }
}
