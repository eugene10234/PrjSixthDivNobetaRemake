using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nobeta.EUG.MBFunctions.PlayerUI
{
    public struct PlayerStatusUpdateEvent
    {
        public float CurrentHealth;
        public float MaxHealth;
        public float CurrentMana;
        public float MaxMana;
        public float CurrentStamina;
        public float MaxStamina;
    }
}
