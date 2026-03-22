using UnityEngine;

namespace Nobeta.EUG.Events
{
    public struct PlayerDamagedEvent
    {
        public int Damage;
        public int CurrentHealth;
        public Vector3 hitPosition;
    }
}
