using Nobeta.EUG.Events;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Services;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.MBFunctions.PlayerUI;
using System;
using System.Collections.Generic;
using UnityEngine;
// ========================================================================
// MeleeAttackState - 近戰攻擊狀態
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    /// <summary>
    /// 近戰攻擊狀態
    /// 使用近戰武器攻擊
    /// </summary>
    public class MeleeAttackState : CombatState
    {
        private Animator _animator;
        private AnimatorStateInfo _stateInfo;
        private int comboCounter = 0;
        private float lastAttackTime = 0f;
        private const float COMBO_WINDOW = 1f;
        private float _lastInputTime = -999f;
        private float _length;
        private float _startStamp;
        private Dictionary<string, AnimationClip> _clipCache;
        private AnimationClip _clip;
        private float _tCount;
        private bool _shouldContinueCombo;
        private Vector3 _TbufmoveDirection;
        private Vector3 _prevMoveDirection;
        private bool _hitEnable;

        public MeleeAttackState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            BaseState prevState = stateMachine.PreviousState;
            if (prevState.GetType() == typeof(MeleeAttackState))
            {
                _prevMoveDirection = _TbufmoveDirection;
            }
            else _prevMoveDirection = Vector3.zero;
            _TbufmoveDirection = Vector3.zero;
            base.Enter();
            _shouldContinueCombo = false;
            ctx.MovementService.RootMotionStart();

            // 檢查連擊
            if (Time.time - lastAttackTime < COMBO_WINDOW)
            {
                ctx.ComboCounter = (ctx.ComboCounter + 1) % 3; // 最多3連擊
            }
            else
            {
                ctx.ComboCounter = 0;
            }
            _clip = ctx.AnimationService.GetClip($"Combo_Attack_03_0{ctx.ComboCounter + 1}");
            _length = _clip.length;
            SetTimer(_length);

            lastAttackTime = Time.time;
            ctx.LastAttackTime = Time.time;

            // 執行攻擊
            PerformMeleeAttack();
            if (ctx.ComboCounter == 2)
                ctx.TimeService.DelayAction(PerformMeleeAttack, 0.22f).Forget();

            PlayAnimation($"MeleeAttack_{ctx.ComboCounter + 1}", 0.1f);
            PlaySound("MeleeAttack");

            // 停止移動
            //ctx.MovementService.ResetVelocity();
            _startStamp = Time.time;
            _tCount = Time.time - _startStamp;
            //ctx.EventBus.Publish(new InteractEreaEnterEvent
            //{
            //    Enter = true,
            //    Message = $"Length:{_length}/C:{_tCount}",
            //    //chest = this

            //});
        }
        public override void Update()
        {
            base.Update();
            ctx.MovementService.StopMovement(ctx.Config.acceleration * 2f);
            //_tCount = Time.time - _startStamp;
            //ctx.EventBus.Publish(new InteractEreaEnterEvent
            //{
            //    Enter = true,
            //    Message = $"L:{_length}/Timer:{StateTimer}",
            //    //chest = this

            //});
            // 應用旋轉
            ctx.MovementService.RotateTowards(
                _prevMoveDirection,
                ctx.Config.rotationSpeed
            );

        }
        public override void Exit()
        {
            base.Exit();
            ctx.MovementService.RootMotionEnd();
            // 攻擊結束後重置連擊計數（如果沒有在連擊窗口內）
            if (Time.time - lastAttackTime >= COMBO_WINDOW)
            {
                ctx.ComboCounter = 0;
            }
        }
        protected override void OnTimerHalfed()
        {
            PerformMeleeEnd();
            if (_shouldContinueCombo)
            {
                stateMachine.ChangeState<MeleeAttackState>();
            }
        }

        protected override void OnTimerCanWalk()
        {
            //攻擊完成
            if (ctx.InputService.MoveInput.magnitude > 0.1f)
            {
                stateMachine.ChangeState<IdleState>();
            }
        }
        protected override void OnTimerExpired()
        {

            //攻擊完成
            //if (ctx.InputService.MoveInput.magnitude > 0.1f)
            //{
            //    stateMachine.ChangeState<WalkingState>();
            //}
            //else
            {
                stateMachine.ChangeState<IdleState>();
            }
        }

        private void PerformMeleeAttack()
        {
            _hitEnable = true;
            // 計算傷害（連擊增加傷害）
            int baseDamage = 150;
            int damage = baseDamage + (ctx.ComboCounter * 100);

            // 發布攻擊事件
            PublishEvent(new MeleeAttackEvent
            {
                Damage = damage,
                ComboCount = ctx.ComboCounter + 1,
                AttackPosition = ctx.Transform.position,
                AttackDirection = ctx.Transform.forward
            });

            //Debug.Log($"執行近戰攻擊！連擊數: {ctx.ComboCounter + 1}, 傷害: {damage}");
        }

        private void PerformMeleeEnd()
        {    if (_hitEnable)
            {
                _hitEnable = false;
                PublishEvent(new MeleeEndEvent { });
            }

        }
        public override void OnAttackInput()
        {
            // 在攻擊動畫後半段可以連擊
            if (StateTimer < _length * 0.4f)
            {
                //stateMachine.ChangeState<MeleeAttackState>();
            }
            else if (StateTimer < (_length * 0.5f + 0.7f))
            {
                _shouldContinueCombo = true;
                _TbufmoveDirection = GetMoveDirection();
            }
        }
        public override void OnMoveInput()
        {
            _TbufmoveDirection = GetMoveDirection();
        }
        public override bool CanTransitionTo(Type stateType)
        {
            if (stateType == typeof(IdleState))
                return StateTimer < _length * 0.4f;
            // 攻擊期間可以連擊
            if (stateType == typeof(MeleeAttackState))
                return StateTimer < _length * 0.5f;

            // 可以被打斷
            if (stateType == typeof(HitState) || stateType == typeof(DyingState))
                return true;

            return StateTimer <= 0;
        }
        protected void ApplyMovementAndRotation(float speed, bool startUpLock = false)
        {
            Vector3 direction = GetMoveDirection();

            if (direction.magnitude > 0.1f)
            {
                if (startUpLock)
                {
                    // 起步鎖定期間減速
                    speed *= 0.15f;
                }
                // 應用移動
                ctx.MovementService.ApplyGroundMovement(
                    direction,
                    speed,
                    ctx.Config.acceleration
                );

                // 應用旋轉
                ctx.MovementService.RotateTowards(
                    direction,
                    ctx.Config.rotationSpeed
                );
            }
        }
        //protected Vector3 GetMoveDirection()
        //{
        //    Vector2 input = ctx.InputService.MoveInput;
        //    return ctx.CameraService.GetCameraRelativeDirection(input);
        //}
    }
}
