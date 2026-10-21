using UnityEngine;

public class PlayerTransformCheck : MonoBehaviour
{
    private MonsterBase _monsterBase;
    private void Awake()
    {
        _monsterBase = GetComponentInParent<MonsterBase>();
        if (_monsterBase == null)
        {
            Logger.LogError("[PlayerTransformCheck] MonsterBase를 가져오지 못했습니다.");
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _monsterBase._playerTransform = collision.transform;
        }
    }
}
