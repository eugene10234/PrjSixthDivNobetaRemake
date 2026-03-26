using UnityEngine;

namespace Nobeta.EUG.Events
{
    public struct SpellCastEvent
    {
        public int SpellPower;
        public Vector3 Direction;
        public float ChargeLevel;
        public Vector3 CastPosition;
    }
}
