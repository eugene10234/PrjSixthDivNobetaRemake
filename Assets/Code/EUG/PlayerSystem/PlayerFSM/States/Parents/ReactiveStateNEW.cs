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
    public abstract class ReactiveStateNEW : BaseState, IUpdatable, IFixedUpdatable
    {
        protected float reactionDuration;
        protected float reactionTimer;
        protected ReactiveStateNEW(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            reactionTimer = reactionDuration;
            // 重置垂直速度（貼地）
            ctx.MovementService.SetVerticalVelocity(-2f);
            // 停止移動
            //ctx.MovementService.StopMovement(ctx.Config.acceleration);
        }

        public virtual void Update()
        {
            UpdateReactionTimer();
        }

        public virtual void FixedUpdate()
        {
            // 默認行為：應用重力
            ctx.MovementService.ApplyGravity(ctx.Config.gravity);

            // 執行移動
            ctx.MovementService.ExecuteMovement();
            //if (context.InputService.IsAimPressed)
            //{
            //    stateMachine.ChangeState<AimingState>();

            //}
        }

        public virtual void UpdateReactionTimer()
        {
            if (reactionTimer > 0)
            {
                reactionTimer -= Time.deltaTime;
            }
            if (reactionTimer <= 0)
            {
                OnReactionComplete();
            }
        }
        protected virtual void OnReactionComplete()
        {
            // 子類覆寫
        }
        public override bool CanTransitionTo(Type stateType)
        {
            // 反應期間只能轉到死亡狀態
            if (stateType == typeof(DyingState))
                return true;

            return reactionTimer <= 0;
        }
        #region Common Input Handlers



        #endregion

     
    }
}
