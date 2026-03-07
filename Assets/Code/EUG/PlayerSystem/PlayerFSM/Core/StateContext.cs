// ============================================================================
// StateContext - 狀態上下文（更新版）
// 純數據容器 + 服務訪問，無邏輯實現
// ============================================================================


using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.FSM.Settings;
using UnityEngine;
using VContainer;

namespace Nobeta.EUG.FSM.Core
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
    public class StateContext
    {
        #region Service Dependencies（服務依賴）

        // 通過構造函數注入，保持不可變性
        public IMovementService MovementService { get; }
        public IPlayerInputService InputService { get; }
        public ICameraService CameraService { get; }
        public IAnimationService AnimationService { get; }
        public IDetectionService Detection { get; }
        public IAudioService AudioService { get; }
        public IEventBus EventBus { get; }
        public DebugServiceMb DebugService { get; }
        public ITimeService TimeService { get; }

        #endregion

        #region Configuration（配置）

        public PlayerConfig Config { get; }

        #endregion

        #region Component References（組件引用 - 只讀）

        public Transform Transform { get; }

        #endregion

        #region Player State Data（玩家狀態數據）

        // 健康和魔力
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set; }
        public int CurrentMana { get; set; }
        public int MaxMana { get; set; }

        // 閃避相關
        public int DodgeCharges { get; set; }
        public float LastDodgeTime { get; set; }

        // 魔法相關
        public float ChargeLevel { get; set; }
        public float ChargeStartTime { get; set; }

        // 戰鬥相關
        public int ComboCounter { get; set; }
        public float LastAttackTime { get; set; }

        #endregion

        #region Constructor（構造函數 - VContainer 注入）

        [Inject]
        public StateContext(
            DebugServiceMb debugServiceMb,
            ITimeService timeService,
            IMovementService movementService,
            IPlayerInputService inputService,
            ICameraService cameraService,
            IAnimationService animationService,
            IDetectionService groundDetection,
            IAudioService audioService,
            IEventBus eventBus,
            PlayerConfig config,
            Transform transform)
        {
            // 注入服務
            DebugService = debugServiceMb;
            TimeService = timeService;
            MovementService = movementService;
            InputService = inputService;
            CameraService = cameraService;
            AnimationService = animationService;
            Detection = groundDetection;
            AudioService = audioService;
            EventBus = eventBus;

            // 注入配置和組件
            Config = config;
            Transform = transform;

            // 初始化玩家屬性
            InitializePlayerStats();
        }

        #endregion

        #region Initialization（初始化）

        private void InitializePlayerStats()
        {
            // 健康和魔力
            MaxHealth = 100;
            CurrentHealth = MaxHealth;
            MaxMana = 100;
            CurrentMana = MaxMana;

            // 閃避
            DodgeCharges = Config.maxDodgeCharges;
            LastDodgeTime = 0f;

            // 魔法
            ChargeLevel = 0f;
            ChargeStartTime = 0f;

            // 戰鬥
            ComboCounter = 0;
            LastAttackTime = 0f;
        }

        #endregion

        #region Utility Properties（工具屬性）

        /// <summary>
        /// 是否在地面上
        /// </summary>
        public bool IsGrounded => Detection.IsGrounded;

        /// <summary>
        /// 是否可以跳躍（包含土狼時間）
        /// </summary>
        public bool CanJump
        {
            get
            {
                float timeSinceGrounded = Time.time - Detection.LastGroundedTime;
                return IsGrounded || timeSinceGrounded < Config.coyoteTime;
            }
        }

        /// <summary>
        /// 是否可以閃避
        /// </summary>
        public bool CanDodge
        {
            get
            {
                return DodgeCharges > 0 &&
                       Time.time - LastDodgeTime > 0.1f;
            }
        }

        /// <summary>
        /// 當前水平速度
        /// </summary>
        public float CurrentSpeed => MovementService.CurrentHorizontalSpeed;

        #endregion
    }
}
