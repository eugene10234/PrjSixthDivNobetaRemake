// ============================================================================
// Core Interfaces - 核心接口定義
// 定義所有服務的契約，遵循 SOLID 的 DIP（依賴倒置原則）
// ============================================================================

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GDCO.Core.Interfaces
{
    // ========================================================================
    // 時間服務接口
    // ========================================================================
    
    /// <summary>
    /// 時間服務接口
    /// </summary>
    public interface ITimeService
    {
        /// <summary>
        /// 當前時間縮放
        /// </summary>
        float TimeScale { get; set; }
        
        /// <summary>
        /// 遊戲時間（受時間縮放影響）
        /// </summary>
        float GameTime { get; }
        
        /// <summary>
        /// 真實時間（不受時間縮放影響）
        /// </summary>
        float RealTime { get; }
        
        /// <summary>
        /// Delta Time
        /// </summary>
        float DeltaTime { get; }
        
        /// <summary>
        /// 暫停遊戲
        /// </summary>
        void Pause();
        
        /// <summary>
        /// 恢復遊戲
        /// </summary>
        void Resume();
        
        /// <summary>
        /// 是否暫停
        /// </summary>
        bool IsPaused { get; }
    }   
}
