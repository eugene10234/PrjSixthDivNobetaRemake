using UnityEngine;
using UnityEngine.Splines;
using VContainer;

namespace Nobeta.Sam.Enemy
{


    public class EnemyVisuals : MonoBehaviour
    {
        private Animator _animator;
        private IEnemyManager _enemyManager;
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        [Inject]
        public void Construct(IEnemyManager enemyManager, BigGhostStat bgs)
        {
            _enemyManager = enemyManager;
        }

        private void OnDestroy()
        {
            
        }

        public void OnHit(Vector3 position)
        {
            _animator.SetTrigger("tHit");
            Vector3 direction = (position - gameObject.transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                gameObject.transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        public void OnDeath()
        {  
          _animator.SetBool("isDead", true);   
        }
    }
}
