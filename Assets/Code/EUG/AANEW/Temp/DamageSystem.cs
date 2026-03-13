using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code.EUG.AANEW.Temp
{
    public static class DamageSystem
    {
        // 這裡存的是介面！
        public static Dictionary<int, IDamageable> Targets = new Dictionary<int, IDamageable>();
        public static bool TryGetTarget (Collider other, out IDamageable target)
        {   
            return Targets.TryGetValue(other.gameObject.GetInstanceID(), out target);
        }
    }
}
