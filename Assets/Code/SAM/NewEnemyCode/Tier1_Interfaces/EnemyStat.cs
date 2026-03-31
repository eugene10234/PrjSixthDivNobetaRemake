using UnityEngine;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Base ScriptableObject for all enemy stat assets.
    /// Every enemy type derives its own stat class from this so shared states
    /// (ChaseState, StaggerState, etc.) can read common fields without casting.
    /// </summary>
    public abstract class EnemyStat : ScriptableObject
    {
        public float MaxHP;
        public float AttackPower;
        public float Speed;
        public float SpacingSpeed;
        public float AggroRange;
        public float ChaseRange;
        public float AttackRange;
        public float AttackSpeed;
        public float DeathDuration = 3;
    }
}
