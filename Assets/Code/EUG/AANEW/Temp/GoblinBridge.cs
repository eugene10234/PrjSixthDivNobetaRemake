using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code.EUG.AANEW.Temp
{
    public class GoblinBridge: EnemyProvider
    {

        private HealthSystemForDummies _dummy;
        //private Goblin _goblin;
        private void Awake()
        {
            _dummy = GetComponent<HealthSystemForDummies>();
        }
        public override void TakeDamage(float damage)
        {
            _dummy.TakeDamage(damage);
            //_goblin.TakeDamage(damage);
        }
    }
}
