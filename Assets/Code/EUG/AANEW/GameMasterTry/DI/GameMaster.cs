// ============================================================================
// Core Services Implementation - 核心服務實現
// 提供接口的具體實現，展示如何實現各個服務
// ============================================================================

using GDCO.Core.Events;
using GDCO.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GDCO.Core
{

    public class GameMaster : ITickable, IFixedTickable, IDisposable, IInitializable,ILateTickable
    {
        private static GameMaster instance;
        //public event Action OnGameInitialized;
        //public event Action OnGameShutdown;
        private readonly IObjectResolver container;
        private readonly GameMasterConfig config;
        private readonly ServiceLifecycleManager serviceRunner;
        private readonly GameContext context;
        //private readonly IGameProvider GameProvider;
        private IEventBus EventBus => context.EventBus;
        private ISaveService SaveService => context.SaveService;
        public GameMaster(GameContext context, ServiceLifecycleManager serviceLifecycleManager)
        {
            this.context = context;
            this.serviceRunner = serviceLifecycleManager;
            if (instance != null) throw new Exception("GameMaster Instance Already Exist");
            instance = this;
        }
        public static IEventBus GetEventBus()
        {
            if (instance == null)
            {
                Debug.LogError("GameMaster 尚未初始化，無法訪問 EventBus。");
                throw new Exception("GameMaster 尚未初始化，無法訪問 EventBus。");
            }
            return instance.EventBus;
        }
        public void Dispose()
        {
            Shutdown();
            instance = null;
        }
        public async void Initialize()
        {

            //ResolveDependencies();
            await context.InitializeAsync();
            //await InitializeAsync();
        }
        public void Tick()
        {
            serviceRunner.OnUpdate();
        }
        public void FixedTick()
        {
            serviceRunner.OnFixedUpdate();
        }
        /// <summary>
        /// 關閉遊戲
        /// </summary>
        public void Shutdown()
        {
            if (!serviceRunner.isInitialized) return;

            Debug.Log("=== GameMaster 關閉開始 ===");

            // 觸發關閉事件
            //OnGameShutdown?.Invoke();
            EventBus?.Publish(new GameShutdownEvent());

            // 保存數據
            SaveService?.SaveAsync("game_state", new GameStateData
            {
                lastPlayTime = DateTime.Now,
                // 其他需要保存的數據...
            }).Wait();

            // 清理所有可清理的服務和列表
            serviceRunner.Dispose();

            Debug.Log("=== GameMaster 關閉完成 ===");
        }

        public void LateTick()
        {
            throw new NotImplementedException();
        }
        ///// <summary>
        ///// 初始化遊戲
        ///// 三階段初始化：PreInitialize → Initialize → PostInitialize
        ///// </summary>
        //private async Task InitializeAsync()
        //{
        //    if (serviceRunner.isInitialized)
        //    {
        //        Debug.LogWarning("GameMaster 已經初始化，跳過重複初始化。");
        //        return;
        //    }

        //    Debug.Log("=== GameMaster 初始化開始 ===");

        //    // 階段 1: 預初始化
        //    await PreInitializeAsync();

        //    // 階段 2: 核心初始化
        //    await InitializeCoreAsync();

        //    // 階段 3: 後初始化
        //    await PostInitializeAsync();

        //    serviceRunner.Initialized();

        //    Debug.Log("=== GameMaster 初始化完成 ===");

        //    // 觸發初始化完成事件
        //    //OnGameInitialized?.Invoke();
        //    _eventBus?.Publish(new GameInitializedEvent());
        //}

        ///// <summary>
        ///// 預初始化 - 載入基礎配置
        ///// </summary>
        //private async Task PreInitializeAsync()
        //{
        //    Debug.Log("[預初始化] 開始");

        //    // 解析依賴（VContainer 自動注入）
        //    ResolveDependencies();

        //    // 載入核心配置
        //    await LoadCoreConfigsAsync();

        //    Debug.Log("[預初始化] 完成");
        //}

        ///// <summary>
        ///// 核心初始化 - 初始化所有服務
        ///// </summary>
        //private async Task InitializeCoreAsync()
        //{
        //    Debug.Log("[核心初始化] 開始");

        //    // 初始化所有可非同步初始化的服務
        //    if (!await serviceRunner.InitializeAsync())
        //    {
        //        Debug.LogError("[核心初始化] 服務初始化失敗");
        //        return;
        //    }
        //    // 設置初始遊戲狀態
        //    _stateManager?.ChangeState(GameState.MainMenu);

        //    Debug.Log("[核心初始化] 完成");
        //}

        ///// <summary>
        ///// 後初始化 - 載入首個場景
        ///// </summary>
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

        //private IEventBus _eventBus;
        //private ISceneService _sceneService;
        //private IGameStateManager _stateManager;
        //private ITimeService _timeService;
        //private ILogService _logService;
        //private IConfigService _configService;
        //private ISaveService _saveService;
        //private IAudioService _audioService;
        //private IUIService _uiService;
        //private IInputService _inputService;
        //private void ResolveDependencies()
        //{
        //    // 從容器中解析服務
        //    // VContainer 會自動注入這些依賴

        //    _eventBus = container.Resolve<IEventBus>();
        //    _sceneService = container.Resolve<ISceneService>();
        //    _stateManager = container.Resolve<IGameStateManager>();
        //    _timeService = container.Resolve<ITimeService>();
        //    _logService = container.Resolve<ILogService>();
        //    _configService = container.Resolve<IConfigService>();

        //    // 可選服務
        //    container.TryResolve(out _saveService);
        //    container.TryResolve(out _audioService);
        //    container.TryResolve(out _uiService);
        //    container.TryResolve(out _inputService);

        //    // 收集需要更新的服務
        //    serviceRunner.BindServices(container.Resolve<IEnumerable<object>>());
        //    //CollectUpdatableServices();
        //}
        ///// <summary>
        ///// 載入核心配置
        ///// </summary>
        //private async Task LoadCoreConfigsAsync()
        //{
        //    // 從 Resources 或 Addressables 載入配置
        //    // 這裡簡化處理，實際項目中可以更複雜

        //    if (config.configsToLoad == null) return;

        //    foreach (var configPath in config.configsToLoad)
        //    {
        //        if (!string.IsNullOrEmpty(configPath))
        //        {
        //            // 載入配置的邏輯
        //            // await configService.LoadConfigAsync<SomeConfig>(configPath);
        //        }
        //    }

        //    await Task.CompletedTask;
        //}
    }
}