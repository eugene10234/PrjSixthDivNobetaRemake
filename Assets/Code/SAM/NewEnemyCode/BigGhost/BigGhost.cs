using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// BigGhost facade — the MonoBehaviour that lives on the prefab root.
    /// Inherits all shared logic from BaseEnemyFacade.
    /// Only responsible for wiring its own concrete dependencies via [Inject].
    /// </summary>
    public class BigGhost : BaseEnemyFacade
    {
        [Inject]
        public void Construct(BigGhostData data, IEnemyStateMachine stateMachine)
        {
            SetDependencies(data, stateMachine);
        }
    }
}
