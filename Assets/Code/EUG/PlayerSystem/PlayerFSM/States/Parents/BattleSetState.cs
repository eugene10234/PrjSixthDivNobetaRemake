using Nobeta.EUG.BulletSystem.OOP.UnityBridge;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.Interfaces;
using System;
using UnityEngine;
// ========================================================================
// GroundedState - 地面狀態父類（更新版）
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    public abstract class BattleSetState : BaseState, IUpdatable
    {
        protected BattleSetState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Set2();
        }

        public virtual void Update()
        {
            MagicHandler();
        }

        private void MagicHandler()
        {
            _isTriggeringAttack = ctx.InputService.IsFirePressed;
            if (!_isTriggeringAttack) return;
            ctx.FireMagic();

        }
        #region Common Input Handlers
        private bool _isTriggeringAttack = false;
        private Action? _inOnFireStart;
        private Action? _inOnFireEnd;
        public Action? InOnFireStart { get => _inOnFireStart; set => _inOnFireStart = value; }
        public Action? InOnFireEnd { get => _inOnFireEnd; set => _inOnFireEnd = value; }
        //public override void OnAttackInput()
        //{
        //    stateMachine.ChangeState<MeleeAttackState>();
        //}
        public override void OnFireStartInput()
        {
            InOnFireStart?.Invoke();
        }
        public override void OnFireEndInput()
        {
            InOnFireEnd?.Invoke();
        }

        #endregion

        #region Helper Methods

        protected void Set1()
        {
            InOnFireStart = null;
            InOnFireEnd = ctx.FireMagic;
        }
        protected void Set2()
        {
            InOnFireStart = () => { _isTriggeringAttack = true; };
            InOnFireEnd = () => { _isTriggeringAttack = false; };
        }
        #endregion
    }
}
