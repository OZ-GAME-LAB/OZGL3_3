using UnityEngine;

public class MonsterSpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject Prefab_Monster;
    [SerializeField] private CircleCollider2D Collider_PlayerCheck;

    private bool _isSpawned = false;
    private int _spawnedMonsterId = -1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SpawnMonster();
        }
    }

    private void SpawnMonster()
    {
        if (_isSpawned == true) return;
        _isSpawned = true;
        if (MonsterSpawnManager.Instance != null)
        {
            _spawnedMonsterId++;
            MonsterSpawnManager.Instance.RequestSpawnEnemy(Prefab_Monster, this.transform.position, this.transform.rotation);
        }
    }

}
