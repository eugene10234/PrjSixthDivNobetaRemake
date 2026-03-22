using Assets.Code.EUG.AANEW.Temp;
using Nobeta.Sam.Pathfinding;
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
        private BigGhostActor _actor;
        private BigGhostStat _bgs;
        private IEnemyStateMachine _esm;
        private IGrid _igrid;

        [Inject]
        public void Construct(BigGhostStat bgs, IEnemyStateMachine esm, BigGhostActor actor, IGrid igrid)
        {
            _esm = esm;
            _bgs = bgs;
            _maxHP = _bgs.MaxHP;
            _currentHP = MaxHP;
            _actor = actor;
            _igrid = igrid;
            _esm.TransitionSubCombatState(SubCombatState.chase);
            _esm.TransitionIdleState(SubIdleState.stop);
            _esm.TransitionState(RootEnemyState.idle);
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

        
        public void InformTarget(GameObject target)
        {
            //_igrid.UpdatePlayerLoc(target.transform.position);
            _actor.GivePlayerInfo(target);
            _esm.OnCombatAlert(target);
        }

        public void InformTargetLost()
        {
            _esm.OnTargetLost();
        }
        public void InformSpacing(Vector3 direction)
        {
            _actor.GiveSpacingDirection(direction);
        }

        public void CheckDeath()
        {
            if (_currentHP <= 0)
            {
                isDead = true;
                OnDeath();
                _actor.OnDeath();
            }
        }

        public void OnVictory()
        {
            _esm.TransitionState(RootEnemyState.victory);
        }
       
        public void OnExitChaseRange()
        {
            _esm.TransitionState(RootEnemyState.idle);
        }
        public void OnDeath()
        {
            _esm.TransitionState(RootEnemyState.death);
        }
        public override void TakeDamage(float amount, Vector3 hitPosition)
        {
            OnDamageTaken(amount, hitPosition);
        }

        private bool isDead = false;
        public void OnDamageTaken(float amount, Vector3 hitPosition)
        {
            if (!isDead)
            { 
                _currentHP -= amount;
                _actor.OnHit(hitPosition);
                _esm.TransitionSubCombatState(SubCombatState.stagger);
                _esm.OnLook();
                CheckDeath();
            }
        }
    }
}
