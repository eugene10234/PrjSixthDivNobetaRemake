using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// MageGhost facade — the MonoBehaviour that lives on the prefab root.
    /// Inherits all shared logic from BaseEnemyFacade.
    /// Only responsible for wiring its own concrete dependencies via [Inject].
    /// </summary>
    public class MageGhost : BaseEnemyFacade
    {
        [Inject]
        public void Construct(MageGhostData data, IEnemyStateMachine stateMachine)
        {
            SetDependencies(data, stateMachine);
        }
    }
}
