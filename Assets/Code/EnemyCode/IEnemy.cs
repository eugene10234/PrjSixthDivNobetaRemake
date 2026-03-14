using UnityEngine;
using System;

namespace Nobeta.Sam.Enemy
{
    public interface IEnemy
    {
        public float MaxHP { get; set; }

        public float CurrentHP { get; set; }
        public float AttackPower { get; set; }
        public void Attack();

        public void OnDeath();

        public void OnDamageTaken(IEnemy enemy, float amount, Vector3 position);

    }
}