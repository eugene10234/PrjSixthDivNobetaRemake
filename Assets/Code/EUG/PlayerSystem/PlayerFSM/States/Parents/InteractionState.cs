using System;
using Nobeta.EUG.BulletSystemV1.OOP.UnityBridge;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.Interfaces;
using UnityEngine;
// ========================================================================
// InteractionState - 互動狀態父類（更新版）
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    /// <summary>
    /// 互動狀態父類
    /// 處理所有與環境/物品互動的共同邏輯
    /// </summary>
    public abstract class InteractionState : BaseState, IUpdatable
    {
        protected float interactionTimer;
        protected float interactionDuration;

        protected InteractionState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            interactionTimer = interactionDuration;

            // 停止移動
            ctx.MovementService.ResetVelocity();
        }

        public virtual void Update()
        {
            UpdateInteractionTimer();
        }

        protected virtual void UpdateInteractionTimer()
        {
            if (interactionTimer > 0)
            {
                interactionTimer -= Time.deltaTime;

                if (interactionTimer <= 0)
                {
                    OnInteractionComplete();
                }
            }
        }

        protected virtual void OnInteractionComplete()
        {
            // 返回待機狀態
            if (ctx.InputService.MoveInput.magnitude > 0.1f)
            {
                stateMachine.ChangeState<WalkingState>();
            }
            else
            {
                stateMachine.ChangeState<IdleState>();
            }
        }

        public override bool CanTransitionTo(System.Type stateType)
        {
            // 互動期間可以被打斷
            if (stateType == typeof(HitState) || stateType == typeof(DyingState))
                return true;

            // 可以被閃避打斷
            if (stateType == typeof(DodgingState))
                return true;

            return interactionTimer <= 0;
        }
    }
}