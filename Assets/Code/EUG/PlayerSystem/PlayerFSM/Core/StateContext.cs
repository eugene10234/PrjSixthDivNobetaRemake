// ============================================================================
// StateContext - 狀態上下文（更新版）
// 純數據容器 + 服務訪問，無邏輯實現
// ============================================================================


using DG.Tweening;
using Nobeta.EUG.BulletSystemV1.OOP;
using Nobeta.EUG.Events;
using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.FSM.Services;
using Nobeta.EUG.FSM.States;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.MBFunctions.PlayerUI;
using Nobeta.EUG.Settings;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            MaxMana = 800;
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

            //CurrentBulletConfig.SetChantCompleteAction(() =>
            //{
            //    EventBus.Publish(new PlayerChantCompleteEvent());
            //});
            //CurrentBulletConfig.SetChantKillAction(() =>
            //{
            //    EventBus.Publish(new PlayerChantKillEvent());
            //});
            CurrentBulletConfig.SetIEventBus(EventBus);
        }

        #endregion

        #region Utility Properties（工具屬性）

        /// <summary>
        /// 是否在地面上
        /// </summary>
        public bool IsGrounded => Detection.IsGrounded;
        public bool IsGroundedEX => (Time.time - Detection.LastGroundedTime) < 10f;
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

        public int CurrentUsableMana
        {
            get
            {
                if (CurrentBulletConfig.ChantFlag)
                {
                    return Mathf.Max(0, CurrentMana - (int)(CurrentBulletConfig.ChantManaCost * CurrentBulletConfig.ChantRatio));
                }
                return CurrentMana;
            }
        }
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
        public void CommandFire()
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
                //CurrentBulletConfig.TryFire(() =>
                //{
                //    EventBus.Publish(new PlayerWeaponFireEvent
                //    {
                //        bulletConfig = CurrentBulletConfig
                //    });
                //    ConsumeMana(CurrentBulletConfig.ManaCost);
                //});
                CurrentBulletConfig.TryFire(evt =>
                {
                    EventBus.Publish(evt);
                    ConsumeMana(evt.FinalManaCost);
                });


                //Debug.Log($"施放魔法！蓄力: {context.ChargeLevel:P0}, 威力: {spellPower}, 魔力消耗: {manaCost}");
            }
            else
            {
                Debug.Log("魔力不足！");
                //PlaySound("ManaNone");
            }
        }
        public void CommandChant()
        {
            CurrentBulletConfig.TryChant();
            StartAsyncChant();
        }

        public void ChantContinue()
        {
            CurrentBulletConfig.ContinueChant();
        }
        public void ChantPause()
        {
            CurrentBulletConfig.PauseChant();
        }
        public void ChantKill()
        {
            CurrentBulletConfig.KillChant(evt => EventBus.Publish(evt));
        }

        private float counter = 0;
        private int Maxcounter = 1;
        public void StartAsyncChant()
        {
            //if (DOTween.IsTweening(CurrentBulletConfig))
            //{
            //    KillAsyncChant();
            //    return;
            //}
            DOVirtual.DelayedCall(0.05f, () =>
            {
                EventBus.Publish(new InteractEreaEnterEvent
                {
                    //InteractObj = this,
                    Message = $"Ratio:{CurrentBulletConfig.ChantRatio}"
                });
            }).SetUpdate(UpdateType.Normal, false)
            .SetLoops(-1, LoopType.Restart);
            //.SetId(CurrentBulletConfig);
        }
        public void KillAsyncChant()
        {
            if (DOTween.IsTweening(CurrentBulletConfig))
            {
                DOTween.Kill(CurrentBulletConfig);
            }
        }

        public void ChangeBullet(BulletType bulletType)
        {
            CurrentBulletConfig = _bulletsDiction[bulletType];
        }
        float lastNotifyTime = 0;
        float interval = 0.05f; // 你想要的 0.05 秒
        public void PRESERVEStartAsyncChant()
        {
            //CurrentBulletConfig.TryChant(() =>
            //{
            //    ChargeStartTime = Time.time;
            //    ChargeRatio = 0f;
            //});
            if (DOTween.IsTweening(CurrentBulletConfig))
            {
                KillAsyncChant();
                return;
            }
            //int counter = 0;
            DOTween.To(() => counter, x => counter = x, CurrentBulletConfig.ManaCost, 0.2f)
                .SetSpeedBased()       // 讓 1f 變成「每秒增加 1」
                .SetEase(Ease.Linear)  // 均速增加
                .SetUpdate(UpdateType.Normal, false)      // 受 TimeScale 影響
                .SetId(CurrentBulletConfig)
                .OnUpdate(() =>
                {
                    //if (Time.time - lastNotifyTime >= interval)
                    //{
                    //    // 每次數值變動都會跑這裡
                    //    EventBus.Publish(new InteractEreaEnterEvent
                    //    {
                    //        Message = $"C:{counter}Ratio:{((float)counter / Maxcounter) * 100}%"
                    //    });
                    //}
                })
                .OnComplete(() =>
                {
                    Debug.Log("計數完成！");
                });
        }

    }
}
