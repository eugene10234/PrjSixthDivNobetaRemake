using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    public class TestBullet : MonoBehaviour
    {
        private float damage = 100f;

        private IEnemyManager _enemyManager;

        [Inject]
        public void Construct(IEnemyManager enemyManager)
        {
            _enemyManager = enemyManager;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Hit!");

            IEnemy enemy = other.GetComponent<IEnemy>();

            if (enemy == null)
                return;

            Vector3 hitPosition = other.ClosestPoint(transform.position);

            enemy.OnDamageTaken(enemy, damage, hitPosition);
            
        }
    }
}
