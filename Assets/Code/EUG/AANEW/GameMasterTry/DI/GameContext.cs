// ============================================================================
// StateContext - 狀態上下文（更新版）
// 純數據容器 + 服務訪問，無邏輯實現
// ============================================================================


using GDCO.Core.Events;
using GDCO.Core.Interfaces;
using GDCO.Core.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;


namespace GDCO.Core
{
    /// <summary>
    /// 狀態上下文
    /// 
    /// 職責：
    /// - 提供服務訪問
    /// - 存儲共享數據
    /// - 提供配置訪問
    /// 
    /// 不負責：
    /// - 任何邏輯實現
    /// - 直接操作 Unity 組件
    /// </summary>
    public class GameContext
    {
        #region Service Dependencies（服務依賴）

        // 通過構造函數注入，保持不可變性
        public IEventBus EventBus { get; private set; }
        public ISceneService SceneService { get; private set; }
        public ITimeService TimeService { get; private set; }
        public ILogService LogService { get; private set; }
        public IConfigService ConfigService { get; private set; }
        public IAudioService AudioService { get; private set; }
        public ISaveService SaveService { get; private set; }
        public IUIService UIService { get; private set; }
        public IInputService InputService { get; private set; }
        public IGameStateManager GameStateManager { get; private set; }

        #endregion
        //public event Action OnGameInitialized;
        //public event Action OnGameShutdown;
        #region Configuration（配置）

        /// <summary>
        /// public GameConfig Config { get; }
        /// </summary>

        #endregion

        private readonly IObjectResolver container;
        private readonly GameMasterConfig config;
        private readonly ServiceLifecycleManager serviceRunner;
        public GameContext(IObjectResolver container, GameMasterConfig config, ServiceLifecycleManager serviceRunner)
        {
            this.container = container;
            this.config = config;
            this.serviceRunner = serviceRunner;
            construct();
        }

        private void construct()
        {
            // 從容器中解析服務
            // VContainer 會自動注入這些依賴
            EventBus = container.Resolve<IEventBus>();
            SceneService = container.Resolve<ISceneService>();
            GameStateManager = container.Resolve<IGameStateManager>();
            TimeService = container.Resolve<ITimeService>();
            LogService = container.Resolve<ILogService>();
            ConfigService = container.Resolve<IConfigService>();
        }
        #region Initialization（初始化）

        /// <summary>
        /// 初始化遊戲
        /// 三階段初始化：PreInitialize → Initialize → PostInitialize
        /// </summary>
        public async Task InitializeAsync()
        {
            if (serviceRunner.isInitialized)
            {
                Debug.LogWarning("GameMaster 已經初始化，跳過重複初始化。");
                return;
            }

            Debug.Log("=== GameMaster 初始化開始 ===");

            // 階段 1: 預初始化
            await PreInitializeAsync();

            // 階段 2: 核心初始化
            await InitializeCoreAsync();

            // 階段 3: 後初始化
            //await PostInitializeAsync();

            serviceRunner.Initialized();

            Debug.Log("=== GameMaster 初始化完成 ===");

            // 觸發初始化完成事件
            //OnGameInitialized?.Invoke();
            EventBus?.Publish(new GameInitializedEvent());
        }

        /// <summary>
        /// 預初始化 - 載入基礎配置
        /// </summary>
        private async Task PreInitializeAsync()
        {
            Debug.Log("[預初始化] 開始");

            // 解析依賴（VContainer 自動注入）
            ResolveDependencies();

            // 載入核心配置
            await LoadCoreConfigsAsync();

            Debug.Log("[預初始化] 完成");
        }

        /// <summary>
        /// 核心初始化 - 初始化所有服務
        /// </summary>
        private async Task InitializeCoreAsync()
        {
            Debug.Log("[核心初始化] 開始");

            // 初始化所有可非同步初始化的服務
            if (!await serviceRunner.InitializeAsync())
            {
                Debug.LogError("[核心初始化] 服務初始化失敗");
                return;
            }
            // 設置初始遊戲狀態
            GameStateManager?.ChangeState(GameState.MainMenu);

            Debug.Log("[核心初始化] 完成");
        }

        /// <summary>
        /// 後初始化 - 載入首個場景
        /// </summary>
        //private async Task PostInitializeAsync()
        //{
        //    //Debug.Log("[後初始化] 開始");

        //    //// 載入主選單場景
        //    //if (!string.IsNullOrEmpty(config.mainMenuSceneName))
        //    //{
        //    //    await sceneService.LoadSceneAsync(config.mainMenuSceneName);
        //    //}

        //    //// 播放主選單音樂
        //    //if (audioService != null && !string.IsNullOrEmpty(config.mainMenuMusic))
        //    //{
        //    //    audioService.PlayMusic(config.mainMenuMusic);
        //    //}

        //    //Debug.Log("[後初始化] 完成");
        //}
        private void ResolveDependencies()
        {
            // 可選服務
            ISaveService _saveService;
            IAudioService _audioService;
            IUIService _uiService;
            IInputService _inputService;
            container.TryResolve(out _saveService);
            container.TryResolve(out _audioService);
            container.TryResolve(out _uiService);
            container.TryResolve(out _inputService);
            SaveService = _saveService;
            AudioService = _audioService;
            UIService = _uiService;
            InputService = _inputService;

            // 收集需要更新的服務
            serviceRunner.BindServices(container.Resolve<IEnumerable<object>>());
        }
        /// <summary>
        /// 載入核心配置
        /// </summary>
        private async Task LoadCoreConfigsAsync()
        {
            // 從 Resources 或 Addressables 載入配置
            // 這裡簡化處理，實際項目中可以更複雜

            if (config.configsToLoad == null) return;

            foreach (var configPath in config.configsToLoad)
            {
                if (!string.IsNullOrEmpty(configPath))
                {
                    // 載入配置的邏輯
                    // await configService.LoadConfigAsync<SomeConfig>(configPath);
                }
            }

            await Task.CompletedTask;
        }

        #endregion   

    }
}
