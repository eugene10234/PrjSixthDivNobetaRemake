using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Shared runtime state for every enemy type.
    /// Plain C# class — no MonoBehaviour, fully unit-testable.
    /// 
    /// Registered Scoped in each enemy's LifetimeScope.
    /// Injected into the facade, state machine, and any state that needs to read/write runtime values.
    /// 
    /// Subclasses add type-specific fields (e.g. BossEnemyData adds CurrentPhase).
    /// </summary>
    public abstract class BaseEnemyData
    {
        protected readonly EnemyStat Stat;

        protected BaseEnemyData(EnemyStat stat)
        {
            Stat = stat;
            CurrentHP = stat.MaxHP;
            Debug.Log("Enemy Data initalization Success");
        }

        public bool isInVinsible;
        public bool isStartAttacking;
        public bool isImmuneStagger;
        public float MaxHP       => Stat.MaxHP;
        public float AttackPower => Stat.AttackPower;

        public float AttackRange => Stat.AttackRange;
        public float CurrentHP   { get; private set; }
        public bool  IsDead      { get; private set; }

        /// <summary>The current combat target. Set by the state machine on OnCombatAlert.</summary>
        public GameObject Target { get; set; }

        /// <summary>
        /// Latest avoidance direction from the spacing system.
        /// Written by the facade's InformSpacing; read by BGMovementActor.
        /// </summary>
        public Vector3 AvoidanceDirection { get; set; }

        public readonly AsyncReactiveProperty<float> HealthPercent = new(1f);

        public void TakeDamage(float amount)
        {
            if (IsDead) return;
            if (isInVinsible) return;
            CurrentHP -= amount;
            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                IsDead = true;
            }
            HealthPercent.Value = CurrentHP / MaxHP;
        }
    }
}
