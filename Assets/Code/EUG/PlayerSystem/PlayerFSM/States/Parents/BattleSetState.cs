using Nobeta.EUG.BulletSystemV1.OOP;
using Nobeta.EUG.BulletSystemV1.OOP.UnityBridge;
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
                ctx.CommandFire();
            }
            GroundDectionRoutine();

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
            if (ctx.CurrentBulletConfig.ChantFlag) return;
            var msNum = ctx.InputService.PressedNumSwitch;
            var te = (int)ctx.CurrentBulletConfig.Type + 1;
            if (msNum != te)
                SetBulletFunc(msNum);

        }

        public override void OnDodgeInput()
        {
            if (ctx.CanDodge)
            {
                if (ctx.StaminaFlag)
                    stateMachine.ChangeState<FellingState>(force: true);
                else
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
        private void GroundDectionRoutine()
        {
            BaseState CurState = stateMachine.CurrentState;
            if (typeof(AerialState).IsAssignableFrom(CurState.GetType())) return;
            //PublishEvent(new InteractEreaEnterEvent
            //{
            //    //InteractObj = this,
            //    Message = $"timeSinceG{timeSinceGrounded>0.15} || isFFGrd{isFarFromGround}"
            //});
            if (ctx.Detection.IsGrounded)
            {
                // 給予一個微小的向下力，確保檢測器能持續觸碰地面
                ctx.MovementService.SetVerticalVelocity(-2f);
            }
            else
            {
                float timeSinceGrounded = Time.time - ctx.Detection.LastGroundedTime;

                // 關鍵：除了時間，還要檢查下方是否真的「空無一物」
                bool isFarFromGround = !ctx.Detection.CheckGroundBelow(1f);
                // 2. 如果不在地面，判斷是「真的在掉落」還是「只是下坡」

                if (timeSinceGrounded > 0.15f && isFarFromGround)
                {
                    // 只有當垂直速度明顯向下，且下方沒有近距離地面時，才進入 FallingState
                    if (ctx.MovementService.CurrentVelocity.y < -2.5f)
                    {
                        stateMachine.ChangeState<FallingState>();
                    }
                }
            }
        }

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
            InOnFireEnd = ctx.CommandFire;
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
            InOnFireEnd = ctx.CommandFire;
        }
        protected void TypeSR()
        {
            IsAutoFiring = false;
            _isTriggeringAttack = false;
            InOnFireStart = null;
            InOnFireEnd = null;
            InOnFireEnd = ctx.CommandFire;
        }
        #endregion
    }
}
