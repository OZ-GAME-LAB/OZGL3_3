using UnityEngine;

public class MonsterSpawnTrigger : MonoBehaviour
{
    [SerializeField] private int targetMonsterId;

    private bool _isTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isTriggered) return;

        if (collision.CompareTag("Player"))
        {
            _isTriggered = true;

            if (monsterObj != null)
            {
                Monster monster = monsterObj.GetComponent<Monster>();
                if (monster != null)
                {
                    monster.Spawn();
                }
            }

            //Destroy(gameObject);
        }
    }
}
