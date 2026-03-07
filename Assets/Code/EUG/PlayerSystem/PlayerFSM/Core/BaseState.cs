// ============================================================================
// BaseState - 更新版狀態基類（支持事件驅動）
// ============================================================================
using System;
using UnityEngine;

namespace Nobeta.EUG.FSM.Core
{
    /// <summary>
    /// 狀態基類
    /// 提供狀態的基本結構和輸入事件處理
    /// </summary>
    public abstract class BaseState : IState, IEnterable
    {
        protected readonly StateContext ctx;
        protected readonly StateMachine stateMachine;

        public virtual string StateName => GetType().Name;
        public virtual IState ParentState => null;

        // 構造函數（VContainer 注入）
        protected BaseState(StateContext context, StateMachine stateMachine)
        {
            this.ctx = context;
            this.stateMachine = stateMachine;
        }

        #region Lifecycle Methods

        public virtual void Enter()
        {
            // 子類覆寫以實現進入邏輯
        }

        public virtual void Exit()
        {
            // 子類覆寫以實現退出邏輯
        }

        #endregion

        #region Input Event Handlers（輸入事件處理器）
        /// <summary>
        /// 處理移動輸入事件
        /// </summary>
        public virtual void OnMoveInput()
        {
            // 默認不處理，子類覆寫
        }

        /// <summary>
        /// 處理停止移動事件
        /// </summary>
        public virtual void OnStopInput()
        {
            // 默認不處理，子類覆寫
        }

        /// <summary>
        /// 處理跳躍輸入事件
        /// </summary>
        public virtual void OnJumpInput()
        {
            // 默認不處理，子類覆寫
        }

        /// <summary>
        /// 處理奔跑輸入事件
        /// </summary>
        public virtual void OnRunInput()
        {
            // 默認不處理，子類覆寫
        }

        /// <summary>
        /// 處理閃避輸入事件
        /// </summary>
        public virtual void OnDodgeInput()
        {
            // 默認不處理，子類覆寫
        }

        /// <summary>
        /// 處理瞄準輸入事件
        /// </summary>
        public virtual void OnAimStartInput()
        {
            // 默認不處理，子類覆寫
        }
        public virtual void OnAimEndInput()
        {
            // 默認不處理，子類覆寫
        }        

        /// <summary>
        /// 處理攻擊輸入事件
        /// </summary>
        public virtual void OnAttackInput()
        {
            // 默認不處理，子類覆寫
        }
        public virtual void OnFireStartInput()
        {
            // 默認不處理，子類覆寫
        }
        #endregion

        #region Transition Control

        /// <summary>
        /// 檢查是否可以轉換到指定狀態
        /// </summary>
        public virtual bool CanTransitionTo(Type stateType)
        {
            return true; // 默認允許所有轉換
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 播放動畫（委託給動畫服務）
        /// </summary>
        protected void PlayAnimation(string animationName, float crossFadeDuration = 0.25f)
        {
            ctx.AnimationService.PlayAnimation(animationName, crossFadeDuration);
        }

        /// <summary>
        /// 播放音效（委託給音頻服務）
        /// </summary>
        protected void PlaySound(string soundName, float volume = 1f)
        {
            ctx.AudioService.PlaySFX(soundName, volume);
        }

        /// <summary>
        /// 發布事件（委託給事件總線）
        /// </summary>
        protected void PublishEvent<T>(T gameEvent) where T : struct
        {
            ctx.EventBus.Publish(gameEvent);
        }

        #endregion
    }
}