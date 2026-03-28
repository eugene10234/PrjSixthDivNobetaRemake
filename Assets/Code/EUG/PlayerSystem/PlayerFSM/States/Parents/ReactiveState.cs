using Nobeta.EUG.BulletSystem.OOP.UnityBridge;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.Interfaces;
using System;
using UnityEngine;

    // ========================================================================
    // ReactiveState - 反應狀態父類
    // ========================================================================
namespace Nobeta.EUG.FSM.States
{
    /// <summary>
    /// 反應狀態 - 被動反應的父類
    /// 包含：Hit, Dying
    /// </summary>
    public abstract class ReactiveState : BaseState, IUpdatable
    {
        protected float reactionDuration;
        protected float reactionTimer;

        protected ReactiveState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine) { }

        public override void Enter()
        {
            reactionTimer = reactionDuration;

            // 停止移動
            ctx.MovementService.StopMovement(ctx.Config.acceleration);
        }

        public virtual void Update()
        {
            UpdateReactionTimer();
        }

        /// <summary>
        /// 更新反應計時器
        /// </summary>
        protected virtual void UpdateReactionTimer()
        {
            if (reactionTimer > 0)
            {
                reactionTimer -= Time.deltaTime;

                if (reactionTimer <= 0)
                {
                    OnReactionComplete();
                }
            }
        }

        /// <summary>
        /// 反應完成時調用
        /// </summary>
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
    }

}
