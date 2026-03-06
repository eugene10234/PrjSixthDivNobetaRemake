
using Nobeta.EUG.FSM.Core;
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
    public abstract class CombatState : BaseState, IUpdatable
    {
        protected float stateTimer;

        protected CombatState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public virtual void Update()
        {
            UpdateTimer();
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
        /// 檢查魔力是否足夠
        /// </summary>
        protected bool HasEnoughMana(int cost)
        {
            return context.CurrentMana >= cost;
        }

        /// <summary>
        /// 消耗魔力
        /// </summary>
        protected void ConsumeMana(int cost)
        {
            context.CurrentMana = Mathf.Max(0, context.CurrentMana - cost);

            // 發布事件
            //PublishEvent(new ManaConsumedEvent
            //{
            //    Amount = cost,
            //    CurrentMana = context.CurrentMana
            //});
        }

        /// <summary>
        /// 獲取瞄準方向
        /// </summary>
        protected Vector3 GetAimDirection()
        {
            return context.CameraService.GetCameraForward();
        }
    }
}
