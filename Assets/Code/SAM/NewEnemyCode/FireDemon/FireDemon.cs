using Nobeta.Sam.Enemy;
using UnityEngine;
using VContainer;
namespace Nobeta.Sam.Enemy
{
    public class FireDemon : BaseEnemyFacade
    {
        private float gizmoradius;
        [Inject]
        public void Construct(FDData data, IEnemyStateMachine stateMachine)
        {
            SetDependencies(data, stateMachine);
            gizmoradius = data.AttackRange;
            Debug.Log("FireDemon Initialized Data and StateMachine");
        }

        private void OnDrawGizmosSelected()
        {
            // Set the color (with some transparency)
            Gizmos.color = new Color(1, 0, 0, 0.5f);

            // Draw a wire circle at the feet of the object
            // Use _stat.AggroRange (or whatever your variable name is)
            Gizmos.DrawWireSphere(transform.position, gizmoradius);
        }
    }
}
