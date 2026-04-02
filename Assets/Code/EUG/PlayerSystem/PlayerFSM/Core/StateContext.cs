// ============================================================================
// StateContext - 狀態上下文（更新版）
// 純數據容器 + 服務訪問，無邏輯實現
// ============================================================================


using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.FSM.Services;
using Nobeta.EUG.Settings;
using Nobeta.EUG.Interfaces;
using UnityEngine;
using VContainer;
using Nobeta.EUG.BulletSystem.OOP;
using Nobeta.EUG.Events;
using System;
using Nobeta.EUG.FSM.States;
using NUnit.Framework;
using System.Collections.Generic;


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
        public int CurrentStamina { get; set; }
        public int MaxStamina { get; set; }
        // 狀態標記
        public Type CurrentSpeedType { get; set; }
        // 閃避相關
        public int DodgeCharges { get; set; }
        public float LastDodgeTime { get; set; }
        public int AirJumpCharges { get; set; }
        public float LastAirJumpTime { get; set; }

        // 魔法相關
        public BulletConfig CurrentBulletConfig { get; set; }
        public float ChargeRatio { get; set; }
        public float ChargeStartTime { get; set; }

        // 戰鬥相關
        public int ComboCounter { get; set; }
        public float LastAttackTime { get; set; }
        public bool StaminaFlag { get; set; }

        public Dictionary<BulletType, BulletConfig> _bulletsDiction;
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
            MaxHealth = 500;
            CurrentHealth = MaxHealth;
            MaxMana = 400;
            CurrentMana = MaxMana;
            MaxStamina = 400;
            CurrentStamina = MaxMana;
            // 狀態標記
            CurrentSpeedType = typeof(IdleState);
            // 閃避
            DodgeCharges = Config.maxDodgeCharges;
            LastDodgeTime = 0f;
            AirJumpCharges = 1;
            LastAirJumpTime = 0f;
            // 魔法
            CurrentBulletConfig = BulletConfig.Normal;
            ChargeRatio = 0f;
            ChargeStartTime = 0f;

            // 戰鬥
            ComboCounter = 0;
            LastAttackTime = 0f;
            StaminaFlag = false;

            _bulletsDiction = new Dictionary<BulletType, BulletConfig>();
            _bulletsDiction.Add(BulletType.Normal, BulletConfig.Normal);
            _bulletsDiction.Add(BulletType.Ice, BulletConfig.IceBall);
            _bulletsDiction.Add(BulletType.Fire, BulletConfig.FireBall);
            _bulletsDiction.Add(BulletType.Lightning, BulletConfig.LightningStrike);
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
        public bool CanAirJump
        {
            get
            {
                return AirJumpCharges > 0 &&
                        Time.time - LastAirJumpTime > 0.1f;
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
        protected bool HasEnoughMana(int cost)
        {
            return CurrentMana >= cost;
        }
        protected void ConsumeMana(int cost)
        {
            CurrentMana = Mathf.Max(0, CurrentMana - cost);

            //// 發布事件
            //PublishEvent(new ManaConsumedEvent
            //{
            //    Amount = cost,
            //    CurrentMana = context.CurrentMana
            //});
        }
        public void ConsumeStamina(int cost)
        {
            CurrentStamina = Mathf.Max(0, CurrentStamina - cost);
        }
        public void FireMagic()
        {
            // if (CurrentBulletConfig.CoolTimerLock) return;
            if (HasEnoughMana(CurrentBulletConfig.ManaCost))
            {
                // 獲取瞄準方向
                //Vector3 aimDirection = GetAimDirection();
                //if (CurrentBulletConfig.CoolTime > 0f)
                //    TimeService.TaskDelayLock(CurrentBulletConfig.CoolTime, (b) => CurrentBulletConfig.CoolTimerLock = b);
                //// 發布施法事件
                ///
                CurrentBulletConfig.TryFire(() =>
                {
                    EventBus.Publish(new PlayerWeaponFireEvent
                    {
                        bulletConfig = CurrentBulletConfig
                    });
                    ConsumeMana(CurrentBulletConfig.ManaCost);
                });
                //Debug.Log($"施放魔法！蓄力: {context.ChargeLevel:P0}, 威力: {spellPower}, 魔力消耗: {manaCost}");
            }
            else
            {
                Debug.Log("魔力不足！");
                //PlaySound("ManaNone");
            }
        }

        public void ChangeBullet(BulletType bulletType)
        {
            CurrentBulletConfig = _bulletsDiction[bulletType];
        }


    }
}
