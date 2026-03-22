using System;
using UnityEngine;

namespace Nobeta.Sam.Enemy
{
    public enum RootEnemyState
    {
        combat,
        idle,
        death,
        victory,
    }

    public enum SubIdleState
    {
        stop,
        look,
        potral,
        home,
    }
    public enum SubCombatState
    {
        attack,
        chase,
        stagger,
    }
    public interface IEnemyStateMachine
    {
        RootEnemyState CurrentState { get; }
        SubCombatState CombatState { get; }
        SubIdleState IdleState { get; }
        public void TransitionState(RootEnemyState state);
        public void TransitionSubCombatState(SubCombatState combatstate);

        public void TransitionIdleState(SubIdleState idlestate);
        public void OnCombatAlert(GameObject player);
        public void OnLook();

        public void OnTargetLost();
    }

}
