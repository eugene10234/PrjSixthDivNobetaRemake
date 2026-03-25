using Nobeta.EUG.BulletSystem.OOP;
using Nobeta.EUG.BulletSystem.OOP.UnityBridge;
using Nobeta.EUG.Events;
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
        private bool AutoFireSwith = false;

        protected BattleSetState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            SetBulletFunc((int)ctx.CurrentBulletConfig.Type + 1);
            //TypePT();
            //TypeAR();
        }

        public virtual void Update()
        {
            DetectionHandler();
        }

        public virtual void DetectionHandler()
        {
            if (IsAutoFiring) _isTriggeringAttack = ctx.InputService.IsFirePressed;
            else _isTriggeringAttack = false;
            if (IsAutoFiring && _isTriggeringAttack)
            {
                ctx.FireMagic();
            }

        }
        //private void MagicHandler()
        //{
        //    _isTriggeringAttack = ctx.InputService.IsFirePressed;
        //    if (!_isTriggeringAttack) return;
        //    ctx.FireMagic();

        //}
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
        public override void OnNumSwitchInput()
        {

            var msNum = ctx.InputService.PressedNumSwitch;
            var te = (int)ctx.CurrentBulletConfig.Type + 1;
            if (msNum != te)
                SetBulletFunc(msNum);

        }

        public override void OnDodgeInput()
        {
            // 空中閃避
            if (ctx.CanDodge)
            {
                stateMachine.ChangeState<DodgingState>(force: true);
            }
        }
        private void SetBulletFunc(int msNum)
        {
            switch (msNum)
            {
                case 1:
                    ctx.ChangeBullet(BulletType.Normal);
                    ctx.EventBus.Publish(new PlayerWeaponChangeEvent { bulletConfig = ctx.CurrentBulletConfig });
                    TypePT();
                    break;
                case 2:
                    ctx.ChangeBullet(BulletType.Ice);
                    ctx.EventBus.Publish(new PlayerWeaponChangeEvent { bulletConfig = ctx.CurrentBulletConfig });
                    TypeAR();
                    break;
                case 3:
                    ctx.ChangeBullet(BulletType.Fire);
                    ctx.EventBus.Publish(new PlayerWeaponChangeEvent { bulletConfig = ctx.CurrentBulletConfig });
                    TypeSG();
                    break;
                case 4:
                    ctx.ChangeBullet(BulletType.Lightning);
                    ctx.EventBus.Publish(new PlayerWeaponChangeEvent { bulletConfig = ctx.CurrentBulletConfig });
                    TypeSR();
                    break;
                default:
                    Debug.LogWarning("OUT!!!!!!!!!!!!!!!!!");
                    break;
            }
        }
        #endregion

        
        #region Helper Methods

        private bool IsAutoFiring = false;
        /// <summary>
        /// 檢查魔力是否足夠
        /// </summary>
        protected bool HasEnoughMana(int cost)
        {
            return ctx.CurrentMana >= cost;
        }
        protected void TypePT()
        {
            IsAutoFiring = false;
            _isTriggeringAttack = false;
            InOnFireStart = null;
            InOnFireEnd = null;
            InOnFireEnd = ctx.FireMagic;
        }
        protected void TypeAR()
        {
            IsAutoFiring = true;
            InOnFireStart = () => { _isTriggeringAttack = true; };
            InOnFireEnd = () => { _isTriggeringAttack = false; };
        }
        protected void TypeSG()
        {
            IsAutoFiring = false;
            _isTriggeringAttack = false;
            InOnFireStart = null;
            InOnFireEnd = null;
            InOnFireEnd = ctx.FireMagic;
        }
        protected void TypeSR()
        {
            IsAutoFiring = false;
            _isTriggeringAttack = false;
            InOnFireStart = null;
            InOnFireEnd = null;
            InOnFireEnd = ctx.FireMagic;
        }
        #endregion
    }
}
