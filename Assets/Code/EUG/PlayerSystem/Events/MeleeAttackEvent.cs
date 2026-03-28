using UnityEngine;

namespace Nobeta.EUG.Events
{
    public struct MeleeAttackEvent
    {
        public int Damage;
        public int ComboCount;
        public Vector3 AttackPosition;
        public Vector3 AttackDirection;
    }
}
