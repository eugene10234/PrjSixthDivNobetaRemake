// ============================================================================
// Core Services Implementation - 核心服務實現
// 提供接口的具體實現，展示如何實現各個服務
// ============================================================================

using GDCO.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GDCO.Core.Services
{
    // ========================================================================
    // LogService - 日誌服務實現
    // ========================================================================
    
    /// <summary>
    /// 日誌服務 - 統一的日誌管理
    /// </summary>
    public class LogService : ILogService
    {
        private readonly bool enableDebugMode;
        private readonly bool verboseLogging;
        
        public bool IsEnabled { get; set; } = true;
        
        public LogService(bool enableDebugMode = false, bool verboseLogging = false)
        {
            this.enableDebugMode = enableDebugMode;
            this.verboseLogging = verboseLogging;
        }
        
        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (!IsEnabled) return;
            
            if (level == LogLevel.Debug && !enableDebugMode) return;
            
            string prefix = GetLogPrefix(level);
            string formattedMessage = $"[{DateTime.Now:HH:mm:ss}] {prefix} {message}";
            
            switch (level)
            {
                case LogLevel.Warning:
                    Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                    Debug.LogError(formattedMessage);
                    break;
                default:
                    Debug.Log(formattedMessage);
                    break;
            }
        }
        
        public void LogWarning(string message)
        {
            Log(message, LogLevel.Warning);
        }
        
        public void LogError(string message, Exception exception = null)
        {
            Log(message, LogLevel.Error);
            if (exception != null)
            {
                Debug.LogException(exception);
            }
        }
        
        private string GetLogPrefix(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => "[DEBUG]",
                LogLevel.Info => "[INFO]",
                LogLevel.Warning => "[WARNING]",
                LogLevel.Error => "[ERROR]",
                _ => ""
            };
        }
    }
}
