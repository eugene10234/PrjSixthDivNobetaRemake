
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.Interfaces;
using System;
using UnityEngine;


namespace Nobeta.EUG.FSM.States
{
    // ========================================================================
    // CombatState - 戰鬥狀態父類（更新版）
    // ========================================================================

    /// <summary>
    /// 戰鬥狀態父類
    /// 處理所有戰鬥狀態的共同邏輯
    /// </summary>
    public abstract class CombatState : BattleSetState, IUpdatable
    {
        protected float stateTimer;

        protected CombatState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Update()
        {
            base.Update();
            UpdateTimer();
        }
        public override void DetectionHandler()
        {
            base.DetectionHandler();
        }
        protected virtual void UpdateTimer()
        {
            if (stateTimer > 0)
            {
                stateTimer -= Time.deltaTime;

                if (stateTimer <= 0)
                {
                    OnTimerExpired();
                }
            }
        }
        protected virtual void OnTimerExpired()
        {
            // 子類覆寫
        }
        /// <summary>
        /// 獲取瞄準方向
        /// </summary>
        protected Vector3 GetAimDirection()
        {
            return ctx.CameraService.GetCameraForward();
        }
        protected Vector3 GetMoveDirection()
        {
            Vector2 input = ctx.InputService.MoveInput;
            return ctx.CameraService.GetCameraRelativeDirection(input);
        }
    }
}
