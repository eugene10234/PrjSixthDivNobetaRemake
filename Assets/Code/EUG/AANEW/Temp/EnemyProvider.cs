using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code.EUG.AANEW.Temp
{
    public abstract class EnemyProvider : MonoBehaviour, IDamageable
    {
        void OnEnable() => DamageSystem.Targets[gameObject.GetInstanceID()] = this;
        void OnDisable() => DamageSystem.Targets.Remove(gameObject.GetInstanceID());
        public abstract void TakeDamage(float amount);
    }
}
