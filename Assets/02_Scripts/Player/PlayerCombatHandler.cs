using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatHandler : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer_Player;
    [SerializeField] private string _targetTag = "Enemy";
    [SerializeField] private Vector2 _rangeOffset = new Vector2(0.4f, 0f);
    [SerializeField] private Vector2 _rangeSize = new Vector2(1.2f, 0.9f);

    private Rigidbody2D _rigidbody2D_Player;

    private readonly List<Collider2D> _overlapColliders = new List<Collider2D>();
    private readonly List<Collider2D> _targetsInRange = new List<Collider2D>();
    private readonly HashSet<NormalMonster> _damagedMonsters = new HashSet<NormalMonster>();

    private int _attackDamage => PlayerStatSet.Instance.AttackDamage;

    private void Awake()
    {
        if (_rigidbody2D_Player == null)
        {
            _rigidbody2D_Player = GetComponent<Rigidbody2D>();
        }

        if (_spriteRenderer_Player == null)
        {
            _spriteRenderer_Player = GetComponentInChildren<SpriteRenderer>();
        }
    }

    public int FindTargetsInRange(List<Collider2D> targets)
    {
        if (targets == null)
        {
            Logger.LogError($"FindTargetsInRange: null입니다.");
            return 0;
        }

        targets.Clear();
        _overlapColliders.Clear();

        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(Physics2D.AllLayers);
        contactFilter.useTriggers = true;

        Physics2D.OverlapBox(
            GetRangeCenter(),
            GetWorldRangeSize(),
            transform.eulerAngles.z,
            contactFilter,
            _overlapColliders);

        foreach (Collider2D overlapCollider in _overlapColliders)
        {
            if (overlapCollider.attachedRigidbody == _rigidbody2D_Player)
            {
                continue;
            }

            if (!HasTargetTagInParents(overlapCollider.transform))
            {
                continue;
            }

            targets.Add(overlapCollider);
        }

        return targets.Count;
    }

    public int ApplyAttackDamage()
    {
        FindTargetsInRange(_targetsInRange);
        _damagedMonsters.Clear();

        foreach (Collider2D targetCollider in _targetsInRange)
        {
            NormalMonster monster = targetCollider.GetComponentInParent<NormalMonster>();
            if (monster == null || !_damagedMonsters.Add(monster))
            {
                continue;
            }

            monster.TakeDamage(_attackDamage);
        }

        return _damagedMonsters.Count;
    }

    public void AnimationAttackHit()
    {
        ApplyAttackDamage();
    }

    private bool HasTargetTagInParents(Transform targetTransform)
    {
        if (string.IsNullOrWhiteSpace(_targetTag))
        {
            Logger.LogError($"HasTargetTagInParents: Target Tag가 비어 있습니다.");
            return false;
        }

        Transform current = targetTransform;
        while (current != null)
        {
            if (current.CompareTag(_targetTag))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private Vector2 GetRangeCenter()
    {
        float facingDirection =
            _spriteRenderer_Player != null && _spriteRenderer_Player.flipX
                ? -1f
                : 1f;

        Vector3 scaledOffset = new Vector3(
            _rangeOffset.x * facingDirection * Mathf.Abs(transform.lossyScale.x),
            _rangeOffset.y * Mathf.Abs(transform.lossyScale.y),
            0f);

        return transform.position + transform.rotation * scaledOffset;
    }

    private Vector2 GetWorldRangeSize()
    {
        Vector3 worldScale = transform.lossyScale;
        return new Vector2(
            _rangeSize.x * Mathf.Abs(worldScale.x),
            _rangeSize.y * Mathf.Abs(worldScale.y));
    }

    private void OnDrawGizmos()
    {
        if (_spriteRenderer_Player == null)
        {
            _spriteRenderer_Player = GetComponentInChildren<SpriteRenderer>();
        }

        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(
            GetRangeCenter(),
            transform.rotation,
            Vector3.one);

        Gizmos.color = new Color(1f, 0f, 0f, 0.18f);
        Gizmos.DrawCube(Vector3.zero, GetWorldRangeSize());
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, GetWorldRangeSize());

        Gizmos.matrix = previousMatrix;
    }
}
