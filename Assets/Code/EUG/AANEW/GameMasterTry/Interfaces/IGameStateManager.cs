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
    // 遊戲狀態接口
    // ========================================================================
    
    /// <summary>
    /// 遊戲狀態管理接口
    /// </summary>
    public interface IGameStateManager
    {
        /// <summary>
        /// 當前遊戲狀態
        /// </summary>
        GameState CurrentState { get; }
        
        /// <summary>
        /// 改變遊戲狀態
        /// </summary>
        void ChangeState(GameState newState);
        
        /// <summary>
        /// 狀態改變事件
        /// </summary>
        event Action<GameState, GameState> OnStateChanged;
    }
    
    /// <summary>
    /// 遊戲狀態枚舉
    /// </summary>
    public enum GameState
    {
        None,
        Initializing,   // 初始化中
        MainMenu,       // 主選單
        Loading,        // 載入中
        InGame,         // 遊戲中
        Paused,         // 暫停
        GameOver,       // 遊戲結束
        Victory,        // 勝利
        Defeat          // 失敗
    }
}
