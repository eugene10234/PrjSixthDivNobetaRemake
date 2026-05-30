// ============================================================================
// GameMasterConfig - GameMaster 配置
// 使用 ScriptableObject 實現配置驅動，方便調整和測試
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace GDCO.Core
{
    /// <summary>
    /// GameMaster 配置
    /// 通過 ScriptableObject 提供可視化配置，遵循配置驅動原則
    /// </summary>
    [CreateAssetMenu(fileName = "GameMasterConfig", menuName = "Game/Core/GameMaster Config")]
    public class GameMasterConfig : ScriptableObject
    {
        [Header("場景配置")]
        [Tooltip("主選單場景名稱")]
        public string mainMenuSceneName = "MainMenu";
        
        [Tooltip("第一個遊戲場景名稱")]
        public string firstGameSceneName = "Level_01";
        
        [Tooltip("載入場景名稱")]
        public string loadingSceneName = "Loading";
        
        [Header("音頻配置")]
        [Tooltip("主選單背景音樂")]
        public string mainMenuMusic = "MainMenuTheme";
        
        [Tooltip("遊戲背景音樂")]
        public string gameMusic = "GameTheme";
        
        [Header("系統開關")]
        [Tooltip("是否啟用存檔系統")]
        public bool enableSaveSystem = true;
        
        [Tooltip("是否啟用音頻系統")]
        public bool enableAudioSystem = true;
        
        [Tooltip("是否啟用輸入系統")]
        public bool enableInputSystem = true;
        
        [Tooltip("是否啟用 UI 系統")]
        public bool enableUISystem = true;
        
        [Tooltip("是否啟用資源管理")]
        public bool enableResourceManagement = true;
        
        [Header("初始化配置")]
        [Tooltip("需要預載入的配置文件路徑")]
        public string[] configsToLoad;
        
        [Tooltip("自定義遊戲模組")]
        public GameModule[] customModules;
        
        [Header("除錯選項")]
        [Tooltip("是否顯示除錯訊息")]
        public bool showDebugInfo = false;
        
        [Tooltip("是否啟用性能監控")]
        public bool enablePerformanceMonitoring = false;
        
        [Tooltip("是否跳過主選單（開發用）")]
        public bool skipMainMenu = false;
    }
    
    /// <summary>
    /// 遊戲模組基類
    /// 允許創建可插拔的遊戲功能模組
    /// </summary>
    [Serializable]
    public abstract class GameModule : ScriptableObject
    {
        [Tooltip("模組名稱")]
        public string moduleName;
        
        [Tooltip("是否啟用此模組")]
        public bool enabled = true;
        
        [Tooltip("模組優先級（數字越小越先初始化）")]
        public int priority = 100;
        
        /// <summary>
        /// 註冊服務到容器
        /// </summary>
        public abstract void RegisterServices(IContainerBuilder builder);
        
        /// <summary>
        /// 模組初始化
        /// </summary>
        public virtual void Initialize() { }
        
        /// <summary>
        /// 模組清理
        /// </summary>
        public virtual void Cleanup() { }
    }
}
