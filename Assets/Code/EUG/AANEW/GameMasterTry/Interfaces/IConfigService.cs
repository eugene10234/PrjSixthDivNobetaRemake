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
    // 配置系統接口
    // ========================================================================
    
    /// <summary>
    /// 配置服務接口
    /// </summary>
    public interface IConfigService
    {
        /// <summary>
        /// 獲取配置
        /// </summary>
        T GetConfig<T>() where T : ScriptableObject;
        
        /// <summary>
        /// 載入配置
        /// </summary>
        Task LoadConfigAsync<T>(string path) where T : ScriptableObject;
        
        /// <summary>
        /// 檢查配置是否已載入
        /// </summary>
        bool HasConfig<T>() where T : ScriptableObject;
    }
}
