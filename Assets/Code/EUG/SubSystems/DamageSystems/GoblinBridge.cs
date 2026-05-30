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

        private MYHealthSystemForDummies _dummy;
        //private Goblin _goblin;
        private void Awake()
        {
            _dummy = GetComponent<MYHealthSystemForDummies>();
        }

        public override void TakeDamage(float amount, Vector3 position)
        {
            //_dummy.TakeDamage(damage);
            ////_goblin.TakeDamage(damage);
        }
    }
}
