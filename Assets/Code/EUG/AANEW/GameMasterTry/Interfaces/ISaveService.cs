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
    // 存檔系統接口
    // ========================================================================
    
    /// <summary>
    /// 存檔服務接口
    /// </summary>
    public interface ISaveService
    {
        /// <summary>
        /// 保存數據
        /// </summary>
        Task SaveAsync<T>(string key, T data) where T : class;
        
        /// <summary>
        /// 載入數據
        /// </summary>
        Task<T> LoadAsync<T>(string key) where T : class;
        
        /// <summary>
        /// 刪除數據
        /// </summary>
        Task DeleteAsync(string key);
        
        /// <summary>
        /// 檢查數據是否存在
        /// </summary>
        Task<bool> ExistsAsync(string key);
        
        /// <summary>
        /// 清除所有數據
        /// </summary>
        Task ClearAllAsync();
        
        /// <summary>
        /// 保存完成事件
        /// </summary>
        event Action<string> OnSaveCompleted;
        
        /// <summary>
        /// 載入完成事件
        /// </summary>
        event Action<string> OnLoadCompleted;
    }  
}
