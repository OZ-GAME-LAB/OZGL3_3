using UnityEngine;

public class ExplosionProjectileSkill : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _lifeTime = 5f;

    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private bool _isExploded = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        Destroy(gameObject, _lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isExploded) return;

        if (collision.CompareTag("Player") || ((1 << collision.gameObject.layer) & LayerMask.GetMask("Ground")) != 0)
        {
            Explode();

            if (collision.CompareTag("Player"))
            {
                // [TODO] 플레이어에게 데미지 전달
            }
        }
    }

    private void Explode()
    {
        _isExploded = true;

        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.simulated = false;
        }

        if (_animator != null)
        {
            _animator.SetTrigger("Explode");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnExplosionAnimationEnd()
    {
        Destroy(gameObject);
    }
}
