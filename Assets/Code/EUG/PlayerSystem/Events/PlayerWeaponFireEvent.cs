using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nobeta.EUG.BulletSystem.OOP;

namespace Nobeta.EUG.Events
{
    public struct PlayerWeaponFireEvent
    {
        public BulletConfig bulletConfig;
        public bool IsSpecial;
        public int FinalManaCost;
    }
   
}
