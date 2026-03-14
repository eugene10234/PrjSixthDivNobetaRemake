using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code.EUG.AANEW.Temp
{
    public interface IDamageable
    {
        void TakeDamage(float damage, Vector3 position);
    }
}
