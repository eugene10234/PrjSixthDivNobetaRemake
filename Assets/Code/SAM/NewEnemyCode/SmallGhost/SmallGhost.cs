using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// SmallGhost facade — the MonoBehaviour that lives on the prefab root.
    /// Inherits all shared logic from BaseEnemyFacade.
    /// </summary>
    public class SmallGhost : BaseEnemyFacade
    {
        [Inject]
        public void Construct(SmallGhostData data, IEnemyStateMachine stateMachine)
        {
            SetDependencies(data, stateMachine);
        }
    }
}
