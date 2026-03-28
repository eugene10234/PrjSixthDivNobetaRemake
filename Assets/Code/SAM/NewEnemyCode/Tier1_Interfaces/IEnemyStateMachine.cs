using UnityEngine;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Contract for every enemy state machine.
    /// The facade (IEnemy implementor) only talks to this — never to concrete state machine types.
    /// </summary>
    public interface IEnemyStateMachine
    {
        /// <summary>Player entered aggro range. State machine decides how to react.</summary>
        void OnCombatAlert(GameObject target);

        /// <summary>Player left chase range.</summary>
        void OnTargetLost();

        /// <summary>Enemy was hit. State machine decides whether to stagger, ignore, etc.</summary>
        void OnHit(Vector3 hitPosition);

        /// <summary>Player was defeated. Transition to victory behaviour.</summary>
        void OnVictory();

        /// <summary>Player was defeated. Transition to victory behaviour.</summary>

        void OnDeath();
    }
}
