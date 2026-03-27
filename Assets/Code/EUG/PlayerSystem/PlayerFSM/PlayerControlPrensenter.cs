using System;
using Nobeta.EUG.BulletSystem.OOP.UnityBridge;
using Nobeta.EUG.Events;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.FSM.Services;
using Nobeta.EUG.FSM.States;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.MBFunctions.PlayerUI;
using Nobeta.EUG.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.EUG.FSM
{
    public class PlayerControlPrensenter : IInitializable, ITickable, IDisposable
    {

        private readonly StateMachine _stateMachine;
        private readonly StateContext _context;
        private readonly ITimeService _timeService;
        private readonly IPlayerInputService _inputService;
        private readonly IDetectionService _detectionService;
        private readonly IEventBus _eventBus;
        private readonly DebugServiceMb _debugService;
        private bool showDebugInfo;
        public PlayerControlPrensenter(IObjectResolver container)
        {
            _stateMachine = container.Resolve<StateMachine>();
            _context = container.Resolve<StateContext>();
            _timeService = container.Resolve<ITimeService>();
            _inputService = container.Resolve<IPlayerInputService>();
            _detectionService = container.Resolve<IDetectionService>();
            _eventBus = container.Resolve<IEventBus>();
            _debugService = container.Resolve<DebugServiceMb>();
        }

        public void Initialize()
        {
            Debug.Log("PlayerController 初始化中（事件驅動模式）");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            showDebugInfo = true;
            if (_debugService == null)
            {
                Debug.LogError("PlayerControlPresenter: _debugService 未注入！");
                return;
            }

            if (_stateMachine == null)
            {
                Debug.LogError("PlayerControlPresenter: _stateMachine 尚未實例化！");
                return;
            }
            _debugService.GUI.Add(DrawDebugInfo);
            //訂閱輸入事件
            //Debug.LogWarning("mbPlayerControllerStart");

            // 設置初始狀態
            _stateMachine.SetInitialState<IdleState>();

            _eventBus.Subscribe<PlayerDamagedEvent>(OnTakeDamage);
            StartUpLock = false;


            Debug.Log("PlayerController 初始化完成（事件驅動模式）");
        }



        public void Dispose()
        {
            // 清理事件訂閱
            _eventBus.Unsubscribe<PlayerDamagedEvent>(OnTakeDamage);
            _stateMachine.Dispose();
            //UnsubscribeFromInputEvents(_inputService);
        }



        public void Tick()
        {
            // 更新檢測
            _detectionService.UpdateGlobalDetection(_context);

            // 更新狀態機
            _stateMachine.Update();

            // 檢查全局狀態轉換
            CheckGlobalTransitions();
            SyncStatusToGUI();
            
        }

        private void SyncStatusToGUI()
        {
            _eventBus.Publish(new PlayerStatusUpdateEvent
            {
                CurrentHealth = _context.CurrentHealth,
                MaxHealth = _context.MaxHealth,
                CurrentMana = _context.CurrentMana,
                MaxMana = _context.MaxMana,
                CurrentStamina = _context.CurrentStamina,
                MaxStamina = _context.MaxStamina
            });
        }

        private void CheckGlobalTransitions()
        {
            // 檢查死亡
            if (_context.CurrentHealth <= 0 && !stateMachine.IsInState<DyingState>())
            {
                stateMachine.ChangeState<DyingState>();
                return;
            }
            if(!stateMachine.IsInState<DyingState>())
            {
                if (!StartUpLock) StatusTimeRecoveryAPI();
            }
            else
            {
                _context.CurrentHealth = 0;        
                 _context.CurrentMana = 0;
                _context.CurrentStamina = 0;
            }

            // 這裡可以添加其他全局轉換邏輯
        }
        private bool StartUpLock = false;
        private const float StartUp_DURATION = 0.5f;
        private void StatusTimeRecoveryAPI()
        {
            _timeService.TaskDelayLock(StartUp_DURATION, (b) => StartUpLock = b);
            RestoreMana(3);
            Heal(1);
            if (_context.CurrentSpeedType == typeof(RunningState))
            {
                _context.ConsumeStamina(16);
            }
            else
            {
                RestoreStamina(24);
            }

        }
        #region Public API（公共接口）
        private void OnTakeDamage(PlayerDamagedEvent evt)
        {
            return;
            TakeDamage(evt.Damage);
        }
        /// <summary>
        /// 受到傷害
        /// </summary>
        public void TakeDamage(int damage)
        {
            if(stateMachine.IsInState<DodgingState>())return;
            _context.CurrentHealth -= damage;

            if (_context.CurrentHealth <= 0)
            {
                _context.CurrentHealth = 0;
                stateMachine.ChangeState<DyingState>();
            }
            else
            {
                if (stateMachine.CurrentState.CanTransitionTo(typeof(HitState)))
                {
                    stateMachine.ChangeState<HitState>();
                }
            }


        }

        /// <summary>
        /// 治療
        /// </summary>
        public void Heal(int amount)
        {
            _context.CurrentHealth = Mathf.Min(
                _context.CurrentHealth + amount,
                _context.MaxHealth
            );
        }

        /// <summary>
        /// 恢復魔力
        /// </summary>
        public void RestoreMana(int amount)
        {
            _context.CurrentMana = Mathf.Min(
                _context.CurrentMana + amount,
                _context.MaxMana
            );
        }
        public void RestoreStamina(int amount)
        {
            _context.CurrentStamina = Mathf.Min(
                _context.CurrentStamina + amount,
                _context.MaxStamina - 1
            );
        }
        /// <summary>
        /// 重生
        /// </summary>
        public void Respawn()
        {
            _context.CurrentHealth = _context.MaxHealth;
            _context.CurrentMana = _context.MaxMana;
            _context.DodgeCharges = _context.Config.maxDodgeCharges;
            _context.MovementService.ResetVelocity();

            stateMachine.ChangeState<IdleState>();
        }

        /// <summary>
        /// 獲取當前狀態名稱
        /// </summary>
        public string GetCurrentStateName()
        {
            return stateMachine.CurrentState?.StateName ?? "None";
        }
        public string GetPreviousStateName()
        {
            return stateMachine.PreviousState?.StateName ?? "None";
        }

        /// <summary>
        /// 檢查是否在指定狀態
        /// </summary>
        public bool IsInState<T>() where T : BaseState
        {
            return stateMachine.IsInState<T>();
        }
        private StateMachine stateMachine => _stateMachine;

        private void DrawDebugInfo()
        {
            GUILayout.Box("=== Player Controller (Event-Driven) ===");

            GUILayout.Label($"<b>CurrentState:</b> {GetCurrentStateName()}");
            GUILayout.Label($"<b>PreviousState:</b> {GetPreviousStateName()}");
            //GUILayout.Label($"Health: {_context.CurrentHealth}/{_context.MaxHealth}  Mana: {_context.CurrentMana}/{_context.MaxMana}");
            //GUILayout.Label($"Dodge Charges: {_context.DodgeCharges}");
            GUILayout.Label($"IsGrounded: {_context.IsGrounded} Speed: {_context.CurrentSpeed:F2}");

            GUILayout.Space(10);

            GUILayout.Label("<b>Input:</b>");
            GUILayout.Label($"Move: {_context.InputService.MoveInput}");
            GUILayout.Label($"Jump Pressed: {_context.InputService.IsJumpPressed}");
            GUILayout.Label($"Run Pressed: {_context.InputService.IsRunPressed}");

            GUILayout.Space(10);

            if (GUILayout.Button("Take Damage (20)"))
            {
                TakeDamage(20);
            }

            if (GUILayout.Button("Respawn"))
            {
                Respawn();
            }

        }



        #endregion
        //#region Input Event Subscription（事件訂閱）

        //private void SubscribeToInputEvents(IPlayerInputService inputService)
        //{
        //    //Debug.LogWarning("SubscribeToInputEvents");
        //    inputService.OnMove += HandleMoveInput;
        //    inputService.OnStop += HandleStopInput;
        //    inputService.OnJump += HandleJumpInput;
        //    inputService.OnRun += HandleRunInput;
        //    inputService.OnDodge += HandleDodgeInput;
        //    inputService.OnAimStart += HandleAimStartInput;
        //    inputService.OnAimEnd += HandleAimEndInput;
        //    inputService.OnFireStart += HandleFireStartInput;
        //    Debug.Log("已訂閱輸入事件");
        //}

        //private void UnsubscribeFromInputEvents(IPlayerInputService inputService)
        //{
        //    inputService.OnMove -= HandleMoveInput;
        //    inputService.OnStop -= HandleStopInput;
        //    inputService.OnJump -= HandleJumpInput;
        //    inputService.OnRun -= HandleRunInput;
        //    inputService.OnDodge -= HandleDodgeInput;
        //    inputService.OnAimStart -= HandleAimStartInput;
        //    inputService.OnAimEnd -= HandleAimEndInput;
        //    inputService.OnFireStart -= HandleFireStartInput;
        //    Debug.Log("已取消訂閱輸入事件");
        //}
        //#endregion
        //#region Input Event Handlers（事件處理器 - 委託給狀態）
        //
        //private void HandleMoveInput() => stateMachine.CurrentState?.OnMoveInput();
        //private void HandleStopInput() => stateMachine.CurrentState?.OnStopInput();
        //private void HandleJumpInput() => stateMachine.CurrentState?.OnJumpInput();
        //private void HandleRunInput() => stateMachine.CurrentState?.OnRunInput();
        //private void HandleDodgeInput() => stateMachine.CurrentState?.OnDodgeInput();
        //private void HandleAimStartInput() => stateMachine.CurrentState?.OnAimStartInput();
        //private void HandleAimEndInput() => stateMachine.CurrentState?.OnAimEndInput();
        //private void HandleFireStartInput() => stateMachine.CurrentState?.OnFireStartInput();
        //#endregion
    }
}
