using Nobeta.EUG.Events;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.FSM.States;
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.EUG.FSM
{
    public class PlayerControlPrensenter : IInitializable, ITickable, IDisposable
    {

        private readonly StateMachine _stateMachine;
        private readonly StateContext _context;
        private readonly IPlayerInputService _inputService;
        private readonly IDetectionService _detectionService;
        private readonly IEventBus _eventBus;
        private readonly mbPlayerController _mb;
        private bool showDebugInfo;
        //public PlayerControlPrensenter(IObjectResolver container)
        //{
        //    _stateMachine = container.Resolve<StateMachine>();
        //    //_context = container.Resolve<StateContext>();
        //    _inputService = container.Resolve<IPlayerInputService>();
        //    _detectionService = container.Resolve<IDetectionService>();
        //    _eventBus = container.Resolve<IEventBus>();
        //    _mb = container.Resolve<mbPlayerController>();
        //    Debug.Log("PlayerControlPrensenter constructed via IObjectResolver");
        //}
        public PlayerControlPrensenter(
            mbPlayerController mb,
            StateMachine stateMachine,
            StateContext context,
            IPlayerInputService inputService,
            IDetectionService groundDetection,
            IEventBus eventBus)
        {
            _mb = mb;
            _stateMachine = stateMachine;
            _context = context;
            _inputService = inputService;
            _detectionService = groundDetection;
            _eventBus = eventBus;

        }
        public void Initialize()
        {
            Debug.Log("PlayerController 初始化中（事件驅動模式）");
            // Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
            //showDebugInfo = true;
            _mb.DebugGUI+=DrawDebugInfo;
            // 訂閱輸入事件
            Debug.LogWarning("mbPlayerControllerStart");

            // 設置初始狀態
            _stateMachine.SetInitialState<IdleState>();

            Debug.Log("PlayerController 初始化完成（事件驅動模式）");
        }
        public void Dispose()
        {
            // 清理事件訂閱
            _stateMachine.Dispose();
            //UnsubscribeFromInputEvents(_inputService);
        }



        public void Tick()
        {
            // 更新地面檢測
            _detectionService.UpdateGroundDetection();

            // 更新狀態機
            _stateMachine.Update();

            // 檢查全局狀態轉換
            CheckGlobalTransitions();
        }
        private void CheckGlobalTransitions()
        {
            // 檢查死亡
            if (_context.CurrentHealth <= 0 && !stateMachine.IsInState<DyingState>())
            {
                stateMachine.ChangeState<DyingState>();
                return;
            }

            // 這裡可以添加其他全局轉換邏輯
        }
        #region Public API（公共接口）

        /// <summary>
        /// 受到傷害
        /// </summary>
        public void TakeDamage(int damage)
        {
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

            // 發布事件
            _eventBus.Publish(new PlayerDamagedEvent
            {
                Damage = damage,
                CurrentHealth = _context.CurrentHealth
            });
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
            if (!Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Box("=== Player Controller (Event-Driven) ===");

            GUILayout.Label($"<b>State:</b> {GetCurrentStateName()}");
            GUILayout.Label($"Health: {_context.CurrentHealth}/{_context.MaxHealth}");
            GUILayout.Label($"Mana: {_context.CurrentMana}/{_context.MaxMana}");
            GUILayout.Label($"Dodge Charges: {_context.DodgeCharges}");
            GUILayout.Label($"Is Grounded: {_context.IsGrounded}");
            GUILayout.Label($"Speed: {_context.CurrentSpeed:F2}");

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

            GUILayout.EndArea();

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
