using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    public class GhostKing : BaseEnemyFacade
    {
        [SerializeField] public List<GameObject> spawnLoc = new List<GameObject>();
        private float gizmoradius;
        [Inject]
        public void Construct(GhostKingData data, IEnemyStateMachine stateMachine)
        {
            SetDependencies(data, stateMachine);
            gizmoradius = data.AttackRange;
            data.minionSpawnLoc = spawnLoc;
            Debug.Log("GhostKing Initialized Data and StateMachine");
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
