// ============================================================================
// Core Interfaces - 核心接口定義
// 定義所有服務的契約，遵循 SOLID 的 DIP（依賴倒置原則）
// ============================================================================

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GDCO.Core.Events
{
    // ========================================================================
    // 事件系統接口
    // ========================================================================
    
    /// <summary>
    /// 事件總線接口 - 全局事件分發系統
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// 訂閱事件
        /// </summary>
        void Subscribe<T>(Action<T> handler, int priority = 0) where T : IGameEvent;
        void Subscribe<T>(Action<T> handler) where T : IGameEvent;
        /// <summary>
        /// 取消訂閱事件
        /// </summary>
        void Unsubscribe<T>(Action<T> handler) where T : IGameEvent;
        
        /// <summary>
        /// 發布事件
        /// </summary>
        void Publish<T>(T gameEvent) where T : IGameEvent;
        
        /// <summary>
        /// 清除所有訂閱
        /// </summary>
        void Clear();
    }
    
    /// <summary>
    /// 遊戲事件標記接口
    /// </summary>
    public interface IGameEvent { }
}
