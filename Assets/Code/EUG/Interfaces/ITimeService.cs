using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
// ========================================================================
// Time Service - 時間服務接口
// ========================================================================
namespace Nobeta.EUG.Interfaces
{
    /// <summary>
    /// 時間服務接口
    /// </summary>
    public interface ITimeService
    {
        /// <summary>
        /// 遊戲時間（受時間縮放影響）
        /// </summary>
        float GameTime { get; }

        /// <summary>
        /// Delta Time
        /// </summary>
        float DeltaTime { get; }

        /// <summary>
        /// Fixed Delta Time
        /// </summary>
        float FixedDeltaTime { get; }

        /// <summary>
        /// 是否暫停
        /// </summary>
        bool IsPaused { get; }    

        /// <summary>
        /// 暫停遊戲
        /// </summary>
        void Pause();

        /// <summary>
        /// 恢復遊戲
        /// </summary>
        void Resume();
        // 傳入傳回 bool 的委託，以及一個取消令牌（防止切換場景時報錯）
        UniTask TaskDelayLock(float seconds, Action<bool> lockCallback, CancellationToken cancellationToken = default);
        UniTaskVoid DelayAction(Action DelayedAction, float duration, CancellationToken cancellationToken = default);
    }
}


