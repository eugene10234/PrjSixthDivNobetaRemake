using TMPro;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// GiantBeastGhost facade — the MonoBehaviour that lives on the prefab root.
    /// Inherits all shared logic from BaseEnemyFacade.
    /// Only responsible for wiring its own concrete dependencies via [Inject].
    /// </summary>
    public class GiantBeastGhost : BaseEnemyFacade
    {
        private GBGStateMachine _gbgStateMachine;

        [Inject]
        public void Construct(GiantBeastGhostData data, GBGStateMachine stateMachine)
        {
            _gbgStateMachine = stateMachine;
            SetDependencies(data, stateMachine);
        }

        private int dodgeCount =3;
        public override void TakeDamage(float amount, Vector3 hitPosition)
        {
            if (Data.IsDead || isDying) return;

            if (dodgeCount > 0 && !Data.isStartAttacking)
            {
                _gbgStateMachine.OnDodge(hitPosition, this.destroyCancellationToken);
                dodgeCount--;
                return; // dodging, skip damage entirely
            }

            // dodgeCount exhausted — take the hit
            Data.TakeDamage(amount);
            dodgeCount = 3;

            if (Data.IsDead)
            {
                isDying = true;
                StateMachine.OnDeath();
                return;
            }

            if (!Data.isImmuneStagger)
                StateMachine.OnHit(hitPosition);
        }
    }
}
