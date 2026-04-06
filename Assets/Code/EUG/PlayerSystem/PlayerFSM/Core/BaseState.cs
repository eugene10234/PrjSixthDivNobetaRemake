// ============================================================================
// BaseState - 更新版狀態基類（支持事件驅動）
// ============================================================================
using Nobeta.EUG.Events;
using System;
using UnityEngine;

namespace Nobeta.EUG.FSM.Core
{
    /// <summary>
    /// 狀態基類
    /// 提供狀態的基本結構和輸入事件處理
    /// </summary>
    public class BaseState : IState, IEnterable
    {
        protected float EnterTime;
        // 設定最小持續時間，0.02s 確保至少跨越 1-2 幀
        public virtual float MinDuration => 0.01f;

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
            EnterTime = Time.time;
            // 子類覆寫以實現進入邏輯
        }

        public virtual void Exit()
        {
            // 子類覆寫以實現退出邏輯
        }
        public virtual bool CanTransitionTo(Type stateType)
        {
            bool aniCanTrans = !ctx.AnimationService.IsInTransition(0);
            bool timePassed = (Time.time - EnterTime) >= MinDuration;
            if (timePassed && aniCanTrans) ctx.MovementService.RootMotionClear();
            return timePassed && aniCanTrans;
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
        public virtual void OnFireEndInput()
        {
            // 默認不處理，子類覆寫
        }
        public virtual void OnChantInput()
        {
            // 默認不處理，子類覆寫
        }
        public virtual void OnNumSwitchInput()
        {
            // 默認不處理，子類覆寫
        }
        public virtual void OnInteractInput()
        {

        }
        #endregion

        #region Transition Control

        /// <summary>
        /// 檢查是否可以轉換到指定狀態
        /// </summary>
        public bool InnerCanTransitionTo(Type stateType, bool force = false)
        {
            //return true; // 默認允許所有轉換
            return force || CanTransitionTo(stateType);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 播放動畫（委託給動畫服務）
        /// </summary>
        protected void PlayAnimation(string animationName, float crossFadeDuration = 0.1f)
        {
            ctx.AnimationService.PlayAnimation(animationName, crossFadeDuration);
        }
        protected void ForcePlayAnimation(string animationName, float crossFadeDuration = 0.1f)
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