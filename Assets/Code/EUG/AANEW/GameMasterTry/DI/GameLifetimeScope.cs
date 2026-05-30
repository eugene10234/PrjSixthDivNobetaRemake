// ============================================================================
// GameMaster - 遊戲總控制器
// 負責協調所有核心服務，管理遊戲生命週期
// 遵循 SOLID 原則，保持簡潔和可擴展性
// ============================================================================


using GDCO.Core.Interfaces;
using GDCO.Core.Services;
using Nobeta.EUG.BulletSystemV2.OOP;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ITimeService = Nobeta.EUG.Interfaces.ITimeService;
using TimeService = Nobeta.EUG.Services.TimeService;

namespace GDCO.Core
{
    /// <summary>
    /// GameMaster - 遊戲主控制器
    /// 
    /// 職責：
    /// 1. 作為 VContainer 的 Root LifetimeScope
    /// 2. 管理遊戲整體生命週期
    /// 3. 協調各個核心服務
    /// 4. 處理場景切換流程
    /// 5. 管理遊戲狀態
    /// 
    /// 設計原則：
    /// - 只做協調，不做實現
    /// - 依賴接口，不依賴具體類
    /// - 通過事件解耦系統
    /// - 保持代碼簡潔（目標 < 200 行）
    /// </summary>
    public class GameLifetimeScope : LifetimeScope
    {
        #region Inspector 配置

        [Header("核心配置")]
        [SerializeField] private GameMasterConfig config;

        [Header("除錯選項")]
        [SerializeField] private bool enableDebugMode = false;
        [SerializeField] private bool verboseLogging = false;

        #endregion

        #region 服務依賴（通過 VContainer 注入）

        // 這些服務會在 Configure 中註冊，並在需要時注入
        private IEventBus eventBus;
        private ISceneService sceneService;
        private ISaveService saveService;
        private IGameStateManager stateManager;
        private ITimeService timeService;
        private ILogService logService;
        private IConfigService configService;

        // 可選服務（可能為 null）
        private IAudioService audioService;
        private IUIService uiService;
        private IInputService inputService;

        #endregion

        #region 生命週期事件

        //public event Action OnGameInitialized;
        //public event Action OnGameShutdown;

        #endregion
        //private static GameMaster _instance;
        //if (_instance != null && _instance != this)
        #region Unity 生命週期

        protected override void Awake()
        {
            // 確保 GameMaster 是單例且不會被銷毀
            if (FindObjectsByType<GameLifetimeScope>(FindObjectsSortMode.None).Length > 1)
            {
                Debug.LogWarning("發現多個 GameMaster 實例，銷毀多餘的實例。");
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);

            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            // ================================================================
            // VContainer 服務註冊
            // 這是 GameMaster 的核心職責之一
            // ================================================================

            if (config == null)
            {
                Debug.LogError("GameMasterConfig 未設置！請在 Inspector 中指定。");
                return;
            }

            // 註冊配置
            builder.RegisterInstance(config);

            // 註冊核心服務（必須）
            RegisterCoreServices(builder);

            // 註冊遊戲服務（可選）
            RegisterGameServices(builder);

            // 註冊管理器
            RegisterManagers(builder);

            // 註冊自定義模組
            RegisterCustomModules(builder);
            // 輸入服務（全局單例）
            builder.Register<PlayerInputService>(Lifetime.Singleton)
                .AsImplementedInterfaces().AsSelf();
            // 推薦寫法：直接註冊類，再告訴 VContainer 它要當作哪些介面使用
            builder.Register<CameraService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<BulletConfigFactory>(Lifetime.Singleton);
            builder.Register<BulletConfigRegistry>(Lifetime.Singleton);
                        builder.RegisterComponentInHierarchy<EventBusProvider>();
            builder.Register<ServiceLifecycleManager>(Lifetime.Singleton);
            builder.Register<GameContext>(Lifetime.Singleton);
            //builder.RegisterComponentInHierarchy<GameProvider>().As<IGameProvider>();
            builder.RegisterEntryPoint<GameMaster>();
        }

        #endregion

        #region 服務註冊

        /// <summary>
        /// 註冊核心服務（必須存在）
        /// </summary>
        private void RegisterCoreServices(IContainerBuilder builder)
        {
            // 事件系統 - 全局通訊
            builder.Register<EventBusConfig>(Lifetime.Scoped);
            builder.Register<IEventBus, EventBus>(Lifetime.Singleton);

            // 場景管理 - 場景載入/卸載
            builder.Register<ISceneService, SceneServiceSimp>(Lifetime.Singleton);

            // 時間服務 - 時間控制
            builder.Register<ITimeService, TimeService>(Lifetime.Singleton);

            // 日誌服務 - 日誌記錄
            builder.Register<ILogService, LogService>(Lifetime.Singleton)
                .WithParameter("enableDebugMode", enableDebugMode)
                .WithParameter("verboseLogging", verboseLogging);

            // 配置服務 - 配置管理
            builder.Register<IConfigService, ConfigService>(Lifetime.Singleton);

            // 遊戲狀態管理
            builder.Register<IGameStateManager, GameStateManager>(Lifetime.Singleton);
        }

        /// <summary>
        /// 註冊遊戲服務（可選，根據配置決定）
        /// </summary>
        private void RegisterGameServices(IContainerBuilder builder)
        {
            // 存檔服務
            if (config.enableSaveSystem)
            {
                builder.Register<ISaveService, SaveService>(Lifetime.Singleton);
            }

            // 音頻服務
            if (config.enableAudioSystem)
            {
                builder.Register<IAudioService, AudioService>(Lifetime.Singleton);
            }

            // 輸入服務
            if (config.enableInputSystem)
            {
                builder.Register<IInputService, InputService>(Lifetime.Singleton);
            }

            // 資源管理
            if (config.enableResourceManagement)
            {
                builder.Register<IResourceService, ResourceService>(Lifetime.Singleton);
            }
        }

        /// <summary>
        /// 註冊管理器
        /// </summary>
        private void RegisterManagers(IContainerBuilder builder)
        {
            // UI 管理器
            if (config.enableUISystem)
            {
                builder.Register<IUIService, UIService>(Lifetime.Singleton);
            }
        }

        /// <summary>
        /// 註冊自定義模組（從配置中讀取）
        /// </summary>
        private void RegisterCustomModules(IContainerBuilder builder)
        {
            if (config.customModules == null) return;

            foreach (var module in config.customModules)
            {
                if (module != null && module.enabled)
                {
                    module.RegisterServices(builder);
                }
            }
        }

        #endregion


        //#region 公共 API

        ///// <summary>
        ///// 開始新遊戲
        ///// </summary>
        //public async Task StartNewGameAsync()
        //{
        //    logService?.Log("開始新遊戲");

        //    stateManager?.ChangeState(GameState.Loading);

        //    // 載入第一個遊戲場景
        //    if (!string.IsNullOrEmpty(config.firstGameSceneName))
        //    {
        //        await sceneService.LoadSceneAsync(config.firstGameSceneName);
        //    }

        //    stateManager?.ChangeState(GameState.InGame);

        //    eventBus?.Publish(new NewGameStartedEvent());
        //}

        ///// <summary>
        ///// 載入遊戲
        ///// </summary>
        //public async Task LoadGameAsync(string saveSlot = "default")
        //{
        //    logService?.Log($"載入遊戲：{saveSlot}");

        //    stateManager?.ChangeState(GameState.Loading);

        //    // 載入存檔數據
        //    if (saveService != null)
        //    {
        //        var saveData = await saveService.LoadAsync<GameSaveData>(saveSlot);
        //        if (saveData != null)
        //        {
        //            // 載入存檔對應的場景
        //            await sceneService.LoadSceneAsync(saveData.sceneName);

        //            // 應用存檔數據
        //            eventBus?.Publish(new GameLoadedEvent { SaveData = saveData });
        //        }
        //    }

        //    stateManager?.ChangeState(GameState.InGame);
        //}

        ///// <summary>
        ///// 暫停遊戲
        ///// </summary>
        //public void PauseGame()
        //{
        //    if (stateManager?.CurrentState != GameState.InGame) return;

        //    timeService?.Pause();
        //    stateManager?.ChangeState(GameState.Paused);
        //    audioService?.PauseAll();

        //    eventBus?.Publish(new GamePausedEvent());
        //}

        ///// <summary>
        ///// 恢復遊戲
        ///// </summary>
        //public void ResumeGame()
        //{
        //    if (stateManager?.CurrentState != GameState.Paused) return;

        //    timeService?.Resume();
        //    stateManager?.ChangeState(GameState.InGame);
        //    audioService?.ResumeAll();

        //    eventBus?.Publish(new GameResumedEvent());
        //}

        ///// <summary>
        ///// 返回主選單
        ///// </summary>
        //public async Task ReturnToMainMenuAsync()
        //{
        //    logService?.Log("返回主選單");

        //    stateManager?.ChangeState(GameState.Loading);

        //    // 卸載當前場景，載入主選單
        //    await sceneService.LoadSceneAsync(config.mainMenuSceneName);

        //    stateManager?.ChangeState(GameState.MainMenu);

        //    if (audioService != null && !string.IsNullOrEmpty(config.mainMenuMusic))
        //    {
        //        audioService.PlayMusic(config.mainMenuMusic);
        //    }
        //}

        ///// <summary>
        ///// 退出遊戲
        ///// </summary>
        //public void QuitGame()
        //{
        //    logService?.Log("退出遊戲");

        //    Shutdown();

        //    #if UNITY_EDITOR
        //    UnityEditor.EditorApplication.isPlaying = false;
        //    #else
        //    Application.Quit();
        //    #endif
        //}

        //#endregion
    }

}
