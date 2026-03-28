using Assets.Code.EUG.AANEW.Temp;
using System;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Shared facade base for every enemy type.
    /// Inherits EnemyProvider so the spawner system can reference it.
    /// Implements IEnemy so detection triggers and the player combat system
    /// can talk to any enemy through one interface.
    ///
    /// Subclasses only need to:
    ///   1. Call SetDependencies() from their [Inject] method.
    ///   2. Override TakeDamage if the type has special damage rules (e.g. shields).
    ///
    /// All shared logic (HP properties, InformTarget, InformSpacing, death guard) lives here.
    /// </summary>
    public abstract class BaseEnemyFacade : EnemyProvider
    {
        protected BaseEnemyData Data;
        protected IEnemyStateMachine StateMachine;
        protected bool isDying = false;

        protected void SetDependencies(BaseEnemyData data, IEnemyStateMachine stateMachine)
        {
            Data = data;
            StateMachine = stateMachine;
        }
        public float MaxHP       => Data.MaxHP;
        public float CurrentHP   => Data.CurrentHP;
        public float AttackPower => Data.AttackPower;

        public override void TakeDamage(float amount, Vector3 hitPosition)
        {
            if (Data.IsDead & !isDying)
            {
                isDying = true;
                Die();
                return;
            }
            else if (!Data.isInVinsible)
            { 
                Data.TakeDamage(amount);
                if (!Data.isImmuneStagger)
                StateMachine.OnHit(hitPosition);
            }
        }
        // ── Unity ─────────────────────────────────────────────────────────────

        private void Update()
        {
            // Player death check — every enemy type needs this
            if (!CheckPlayerState())
            StateMachine.OnVictory();
        }

        protected virtual bool CheckPlayerState()
        {
            // IsPlayerLive comes from EnemyProvider
            return IsPlayerLive;
        }

        // communicate with outside system
        public event Action OnDeath;

        // called inside your existing death path
        protected void Die()
        {
            OnDeath?.Invoke();
            StateMachine.OnDeath();
        }
    }
}
