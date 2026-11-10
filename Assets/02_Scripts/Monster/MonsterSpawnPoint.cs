using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject Prefab_Monster;
    [SerializeField] private CircleCollider2D Collider_PlayerCheck;
    [SerializeField] private string _defeatEventId;

    private bool _isSpawned = false;

    private void OnDisable()
    {
        _isSpawned = false;
    }

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
            GameObject monsterObject = MonsterSpawnManager.Instance.RequestSpawnEnemy(
                Prefab_Monster, transform.position, transform.rotation);
            if (!string.IsNullOrEmpty(_defeatEventId) && monsterObject != null &&
                monsterObject.TryGetComponent(out MonsterBase monster))
            {
                monster.OnDied += RecordDefeat;
            }
        }
    }

    private void RecordDefeat()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Npc.Dialogue.RecordEvent(_defeatEventId);
        }
    }
}
