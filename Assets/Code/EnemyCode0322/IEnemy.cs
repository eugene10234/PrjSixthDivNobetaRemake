using UnityEngine;
using System;

namespace Nobeta.Sam.Enemy
{
    public interface IEnemy
    {
        public float MaxHP { get; set; }

        public float CurrentHP { get; set; }
        public float AttackPower { get; set; }
        public void OnDeath();

        public void OnDamageTaken(float amount, Vector3 hitPosition);

    }
}