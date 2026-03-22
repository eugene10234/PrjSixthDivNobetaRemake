using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.Sam.Enemy
{
    public class BigGhostStateMachine : IEnemyStateMachine, IFixedTickable, IDisposable, IStartable
    {
        private BigGhostStat _bgs;
        private GameObject _target;
        private BigGhostActor _actor;
     
        [Inject]

        public void Construct(BigGhostStat bgs, BigGhostActor actor)
        { 
            _bgs = bgs;
            _actor = actor;
        }

        public void Start()
        {
            _actor.OnReturnHome += TransitiontoStop;
        }
        
        public RootEnemyState CurrentState { get; private set; }
        public SubCombatState CombatState { get; private set; }

        public SubIdleState IdleState { get; private set; }

        public void TransitionState(RootEnemyState state)
        {
            CurrentState = state;
        }
        public void TransitionSubCombatState(SubCombatState combatstate)
        {
            CombatState = combatstate;
            Debug.Log($"<color=green>Current State: {combatstate}</color>");
        }
        public void TransitionIdleState(SubIdleState idlestate)
        {
            IdleState = idlestate;
            Debug.Log($"<color=green>Current State: {idlestate}</color>");
        }

        public RootEnemyState GetCurrentState()
        { 
            return CurrentState;
        }

        public SubIdleState GetIdleState() 
        {
            return IdleState;
        }
        public SubCombatState GetCombatState() 
        {
            return CombatState;
        }


        public void FixedTick()
        {
            _actor.NodeUpdate();
            switch (GetCurrentState()) 
            {
                case RootEnemyState.idle:
                    UpdateIdleState();
                    break;
                case RootEnemyState.combat:
                    UpdateCombatState();
                    break;
                case RootEnemyState.death:
                    break;
                case RootEnemyState.victory:
                    break;
                default:
                    Debug.Log("StateMachine Error");
                        break;
            }
        }

        //Combat

       
        public void OnCombatAlert(GameObject player)
        {
            _target = player;
            if (CurrentState == RootEnemyState.death || CurrentState == RootEnemyState.victory) return;
            TransitionState(RootEnemyState.combat);
        }

        public void OnTargetLost()
        { 
            TransitionIdleState(SubIdleState.home);
            TransitionState(RootEnemyState.idle);
        }
        public void UpdateCombatState()
        {
            float range = Vector3.Distance(_actor.transform.position, _target.transform.position);
            if (range <= _bgs.AttackRange)
            {
                TransitionSubCombatState(SubCombatState.attack);
            }
            else if (range > _bgs.AttackRange && range <= _bgs.ChaseRange)
            {
                TransitionSubCombatState(SubCombatState.chase);
            }
            switch (GetCombatState())
            {
                case SubCombatState.attack:
                    _actor.OnAttack(_target);
                    break;
                case SubCombatState.stagger:
                    break;
                case SubCombatState.chase:
                     _actor.Move();
                    break;
            }
        }

        //Idle

        public void TransitiontoStop()
        {
            TransitionIdleState(SubIdleState.stop);
        }
        public void UpdateIdleState()
        {
            switch (GetIdleState())
            {
                case SubIdleState.stop:
                    _actor.Idle();
                    break;
                case SubIdleState.home:
                    _actor.ReturnHome();
                    break;
                default:
                    break;
            }
        }

        
        public void OnLook()
        {
            if (CurrentState != RootEnemyState.combat)
            {
                var player = _actor.Look();
                if (player != null)
                { 
                    _target = player;
                    CurrentState= RootEnemyState.combat;
                    TransitionSubCombatState(SubCombatState.chase);
                } 
            }
        }

        public void Dispose()
        {
            _actor.OnReturnHome -= TransitiontoStop;
        }
    }
}