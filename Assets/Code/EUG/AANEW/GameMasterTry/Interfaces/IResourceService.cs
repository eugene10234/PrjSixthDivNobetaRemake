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
    // 資源管理接口
    // ========================================================================
    
    /// <summary>
    /// 資源服務接口
    /// </summary>
    public interface IResourceService
    {
        /// <summary>
        /// 載入資源（異步）
        /// </summary>
        Task<T> LoadAsync<T>(string path) where T : UnityEngine.Object;
        
        /// <summary>
        /// 載入多個資源（異步）
        /// </summary>
        Task<T[]> LoadAllAsync<T>(string path) where T : UnityEngine.Object;
        
        /// <summary>
        /// 實例化預製體
        /// </summary>
        Task<GameObject> InstantiateAsync(string path, Transform parent = null);
        
        /// <summary>
        /// 釋放資源
        /// </summary>
        void Release(UnityEngine.Object asset);
        
        /// <summary>
        /// 釋放所有資源
        /// </summary>
        void ReleaseAll();
    }

}
