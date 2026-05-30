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
    // 日誌系統接口
    // ========================================================================
    
    /// <summary>
    /// 日誌服務接口
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// 記錄訊息
        /// </summary>
        void Log(string message, LogLevel level = LogLevel.Info);
        
        /// <summary>
        /// 記錄警告
        /// </summary>
        void LogWarning(string message);
        
        /// <summary>
        /// 記錄錯誤
        /// </summary>
        void LogError(string message, Exception exception = null);
        
        /// <summary>
        /// 是否啟用日誌
        /// </summary>
        bool IsEnabled { get; set; }
    }
    
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
}
