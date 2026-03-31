using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code.EUG.AANEW.Temp
{
    public static class EnemyDamageSystem
    {
        public static Dictionary<int, IDamageable> Targets = new Dictionary<int, IDamageable>();
        public static bool TryGetTarget (GameObject gameObject, out IDamageable target)
        {   
            return Targets.TryGetValue(gameObject.GetInstanceID(), out target);
        }
    }
}
