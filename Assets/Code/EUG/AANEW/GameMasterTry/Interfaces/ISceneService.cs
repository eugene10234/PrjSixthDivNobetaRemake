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
    // 場景管理接口
    // ========================================================================
    
    /// <summary>
    /// 場景服務接口 - 管理場景載入/卸載
    /// </summary>
    public interface ISceneService
    {
        /// <summary>
        /// 當前活動場景
        /// </summary>
        Scene CurrentScene { get; }
        
        /// <summary>
        /// 是否正在載入場景
        /// </summary>
        bool IsLoading { get; }
        
        /// <summary>
        /// 載入進度 (0-1)
        /// </summary>
        float LoadingProgress { get; }
        
        /// <summary>
        /// 載入場景（異步）
        /// </summary>
        Task<Scene> LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single);
        
        /// <summary>
        /// 卸載場景（異步）
        /// </summary>
        Task UnloadSceneAsync(string sceneName);
        
        /// <summary>
        /// 場景載入開始事件
        /// </summary>
        event Action<string> OnSceneLoadStarted;
        
        /// <summary>
        /// 場景載入完成事件
        /// </summary>
        event Action<Scene> OnSceneLoadCompleted;
        
        /// <summary>
        /// 場景載入進度事件
        /// </summary>
        event Action<float> OnLoadingProgress;
    }  
}
