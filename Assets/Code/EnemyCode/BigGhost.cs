using Assets.Code.EUG.AANEW.Temp;
using System;
using System.Collections;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;
using static UnityEngine.EventSystems.EventTrigger;

namespace Nobeta.Sam.Enemy
{
    public class BigGhost : EnemyProvider, IEnemy 
    {
        private float _maxHP;
        private float _currentHP;
        private float _attackPower;
        private EnemyVisuals _visuals;
        private BigGhostStat _bgs;
        private IEnemyManager _enemyManager;

        [Inject]
        public void Construct(IEnemyManager enemyManager, BigGhostStat bgs)
        {
            _bgs = bgs;
            MaxHP = _bgs.MaxHP;
            _currentHP = MaxHP;
            _enemyManager = enemyManager;
            _visuals = GetComponentInChildren<EnemyVisuals>();
        }
        private void OnDestroy()
        {
                
        }

        void Awake()
        {
        }
        public float MaxHP
        {
            get => _maxHP;
            set => _maxHP = value;
        }

        public float CurrentHP
        {
            get => _currentHP;
            set => _currentHP = value;
        }

        public float AttackPower
        {
            get => _attackPower;
            set => _attackPower = value;
        }

        public void Attack()
        { 
        
        }

        public void CheckDeath()
        {
            if (_currentHP <= 0)
            {
                OnDeath();
            }
        }
        public void OnDeath()
        {
            _visuals.OnDeath();
        }
        public override void TakeDamage(float amount, Vector3 hitPosition)
        {
            _currentHP -= amount;
            _visuals.OnHit(hitPosition);
            CheckDeath();
        }
        public void OnDamageTaken(IEnemy enemy, float amount, Vector3 hitPosition)
        {
            //_currentHP -= amount;
            //_visuals.OnHit(hitPosition);
            //CheckDeath();
        }
    }
}
