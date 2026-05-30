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
    // UI 管理接口
    // ========================================================================
    
    /// <summary>
    /// UI 服務接口
    /// </summary>
    public interface IUIService
    {
        /// <summary>
        /// 顯示 UI 面板
        /// </summary>
        Task ShowPanelAsync<T>(object data = null) where T : UIPanel;
        
        /// <summary>
        /// 隱藏 UI 面板
        /// </summary>
        Task HidePanelAsync<T>() where T : UIPanel;
        
        /// <summary>
        /// 獲取面板實例
        /// </summary>
        T GetPanel<T>() where T : UIPanel;
        
        /// <summary>
        /// 檢查面板是否顯示
        /// </summary>
        bool IsPanelShown<T>() where T : UIPanel;
        
        /// <summary>
        /// 顯示通知
        /// </summary>
        void ShowNotification(string message, float duration = 3f);
        
        /// <summary>
        /// 顯示確認對話框
        /// </summary>
        Task<bool> ShowConfirmDialogAsync(string title, string message);
    }
    
    /// <summary>
    /// UI 面板基類（標記）
    /// </summary>
    public abstract class UIPanel : MonoBehaviour
    {
        public abstract Task ShowAsync(object data = null);
        public abstract Task HideAsync();
    }
}
