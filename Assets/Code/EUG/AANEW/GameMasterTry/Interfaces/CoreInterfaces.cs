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
    // 生命週期接口
    // ========================================================================
    
    /// <summary>
    /// 可初始化的服務接口
    /// </summary>
    public interface IInitializeAsync
    {
        /// <summary>
        /// 初始化服務
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }
    }
    
    /// <summary>
    /// 可更新的服務接口
    /// </summary>
    public interface IUpdatable
    {
        /// <summary>
        /// 每幀更新
        /// </summary>
        void Update();
    }
    
    /// <summary>
    /// 可固定更新的服務接口
    /// </summary>
    public interface IFixedUpdatable
    {
        /// <summary>
        /// 固定時間間隔更新
        /// </summary>
        void FixedUpdate();
    }
    
    /// <summary>
    /// 可清理的服務接口
    /// </summary>
    public interface IDisposableService : IDisposable
    {
        /// <summary>
        /// 清理服務資源
        /// </summary>
        void Cleanup();
    }
}
