using Nobeta.Sam.Pathfinding;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using VContainer.Unity;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Shared state machine base for every enemy type.
    /// Owns the state registry, transition logic, and FixedTick dispatch.
    ///
    /// Subclasses must:
    ///   1. Call RegisterState() for every state in their [Inject] method.
    ///   2. Implement Start() to set the initial state via TransitionTo&lt;T&gt;().
    ///   3. Implement OnCombatAlert, OnTargetLost, OnHit, OnVictory.
    ///
    /// Nothing in this class knows about specific enemy types or concrete states.
    /// </summary>
    public abstract class BaseStateMachine : IEnemyStateMachine, IFixedTickable, IStartable, IDisposable
    {
        private readonly Dictionary<Type, IState> _states = new();
        protected IState CurrentState { get; private set; }

        // ── Registration ──────────────────────────────────────────────────────

        protected void RegisterState<T>(T state) where T : IState
        {
            _states[typeof(T)] = state;
        }

        protected void TransitionTo<T>() where T : IState
        {
            if (!_states.TryGetValue(typeof(T), out var next))
            {
                UnityEngine.Debug.LogError($"[StateMachine] State {typeof(T).Name} was not registered.");
                return;
            }
            CurrentState?.OnExit();
            CurrentState = next;
            CurrentState.OnEnter();
        }

        // ── VContainer lifecycle ──────────────────────────────────────────────

        public abstract void Start();

        public virtual void FixedTick()
        {
            CurrentState?.OnFixedTick();
            if (CurrentState != null)
            {
                Debug.Log($"<color=green>{CurrentState}</color>");
            }
        }

        public virtual void Dispose() { }

        // ── IEnemyStateMachine ────────────────────────────────────────────────

        //idle state already scan for player, this never calls.
        public abstract void OnCombatAlert(GameObject target);
        public abstract void OnTargetLost();
        public abstract void OnHit(Vector3 hitPosition);
        public abstract void OnVictory();

        public abstract void OnDeath();
    }
}
