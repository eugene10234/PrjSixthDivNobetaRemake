// ============================================================================
// StateMachine - 狀態機核心（VContainer 版本）
// ============================================================================

using Nobeta.EUG.Events;
using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.FSM.Services;
using Nobeta.EUG.FSM.States;
using Nobeta.EUG.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.EUG.FSM.Core
{
    /// <summary>
    /// 狀態機
    /// 使用 VContainer 解析狀態，支持依賴注入
    /// </summary>
    public class StateMachine : IDisposable
    {
        private readonly IPlayerInputService _inputService;
        private readonly IMovementService _movementService;
        private readonly IEventBus _eventBus;
        private StateContext _context;
        private readonly IObjectResolver container;
        private readonly Dictionary<Type, BaseState> stateCache;
        private readonly Queue<Type> stateHistory;
        private readonly int MaxStateHistoryCount = 30;

        private BaseState currentState;
        private BaseState previousState;

        public BaseState CurrentState => currentState;
        public BaseState PreviousState => previousState;

        public event Action<BaseState, BaseState> OnStateChanged;

        // 構造函數注入
        [Inject]
        public StateMachine(IObjectResolver container)
        {
            this.container = container;
            _inputService = container.Resolve<IPlayerInputService>();
            _movementService = container.Resolve<IMovementService>();
            _eventBus = container.Resolve<IEventBus>();
            _context = container.Resolve<StateContext>();
            SubscribeInputEvents();
            SubscribeAPIEvents();
            this.stateCache = new Dictionary<Type, BaseState>();
            this.stateHistory = new Queue<Type>();
        }
        private Action ActionToInitialState;
        private void ToInitialState() => ActionToInitialState?.Invoke();

        /// <summary>
        /// 設置初始狀態
        /// </summary>
        public void SetInitialState<T>() where T : BaseState
        {
            _movementService.SetGround();
            ActionToInitialState = ChangeState<T>;
            ToInitialState();
        }
        public void ChangeStateByParent<T>(Type childrenType) where T : BaseState
        {
            if (!typeof(T).IsAssignableFrom(childrenType) || childrenType.IsAbstract)
            {
                Debug.LogWarning($"{childrenType.Name} 不是合法的狀態類別！");
                ToInitialState();
                return;
            }
            ChangeState(childrenType);

        }
        public void ChangeState<T>() where T : BaseState
        {
            ChangeState(typeof(T));
        }
        public void ChangeState<T>(bool force) where T : BaseState
        {
            ChangeState(typeof(T), force);
        }
        /// <summary>
        /// 改變狀態
        /// </summary>
        private void ChangeState(Type stateType, bool force = false)
        {
            // 1. 避免重複切換
            if (typeof(GroundedState).IsAssignableFrom(stateType) && CurrentState?.GetType() == stateType) return;
            if (stateType.IsAbstract) { ToInitialState(); return; }

            // 從容器解析狀態（VContainer 自動注入依賴）
            BaseState newState = GetOrCreateState(stateType);

            if (newState == null)
            {
                Debug.LogError($"無法創建狀態：{stateType.Name}");
                return;
            }

            // 檢查是否可以轉換
            if (currentState != null && !currentState.InnerCanTransitionTo(stateType, force))
            {
                Debug.LogWarning($"無法從 {currentState.StateName} 轉換到 {newState.StateName}");
                return;
            }

            // 退出當前狀態
            if (currentState != null)
            {
                currentState.Exit();
                stateHistory.Enqueue(currentState.GetType());

                // 限制歷史記錄大小
                while (stateHistory.Count > MaxStateHistoryCount)
                {
                    //var temp = stateHistory.ToArray();
                    //stateHistory.Clear();

                    stateHistory.Dequeue();

                }
            }

            previousState = currentState;
            currentState = newState;

            // 進入新狀態
            currentState.Enter();

            // 觸發事件
            OnStateChanged?.Invoke(previousState, currentState);

#if UNITY_EDITOR
            Debug.Log($"[FSM] {previousState?.StateName ?? "None"} → {currentState.StateName}");
#endif
        }

        /// <summary>
        /// 獲取或創建狀態（使用緩存）
        /// </summary>
        private BaseState GetOrCreateState(Type stateType)
        {
            // 檢查緩存
            if (stateCache.TryGetValue(stateType, out BaseState cachedState))
            {
                return cachedState;
            }

            // 從 VContainer 解析（自動注入依賴）
            try
            {
                BaseState newState = container.Resolve(stateType) as BaseState;

                if (newState != null)
                {
                    // 緩存狀態（可選：如果想要每次都創建新實例，就不要緩存）
                    stateCache[stateType] = newState;
                    return newState;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"解析狀態失敗：{stateType.Name}\n{e.Message}");
            }

            return null;
        }

        /// <summary>
        /// 更新當前狀態
        /// </summary>
        public void Update()
        {
            if (currentState is IFixedUpdatable fixedUpdatable)
            {
                fixedUpdatable.FixedUpdate();
            }
            if (currentState is IUpdatable updatable)
            {
                updatable.Update();
            }
        }

        /// <summary>
        /// 固定更新當前狀態
        /// </summary>
        public void FixedUpdate()
        {
            //if (currentState is IFixedUpdatable fixedUpdatable)
            //{
            //    fixedUpdatable.FixedUpdate();
            //}
        }

        /// <summary>
        /// 檢查是否在指定狀態或其子類型
        /// </summary>
        public bool IsInState<T>() where T : BaseState
        {
            if (currentState == null) return false;

            Type targetType = typeof(T);
            Type currentType = currentState.GetType();

            // 檢查當前類型或父類型
            while (currentType != null && currentType != typeof(object))
            {
                if (currentType == targetType)
                    return true;

                currentType = currentType.BaseType;
            }

            return false;
        }

        /// <summary>
        /// 獲取狀態歷史
        /// </summary>
        public IEnumerable<Type> GetStateHistory()
        {
            return stateHistory;
        }
        //public Type GetLastGroundedState()
        //{
        //    Type lastGroundedState = GetStateHistory().Where(type => typeof(GroundedState).IsAssignableFrom(type)).LastOrDefault();
        //    return lastGroundedState;
        //}
        /// <summary>
        /// 清除狀態緩存（用於釋放內存）
        /// </summary>
        public void ClearCache()
        {
            stateCache.Clear();
            stateHistory.Clear();
        }
        //========================================================================
        public void Dispose()
        {
            UnsubscribeInputEvents();
            UnsubscribeAPIEvents();
        }
        private void SubscribeInputEvents()
        {
            //Debug.LogWarning("SubscribeToInputEvents");
            _inputService.OnMove += HandleMoveInput;
            _inputService.OnStop += HandleStopInput;
            _inputService.OnJump += HandleJumpInput;
            _inputService.OnRun += HandleRunInput;
            _inputService.OnDodge += HandleDodgeInput;
            _inputService.OnAimStart += HandleAimStartInput;
            _inputService.OnAimEnd += HandleAimEndInput;
            _inputService.OnFireStart += HandleFireStartInput;
            _inputService.OnFireEnd += HandleFireEndInput;
            _inputService.OnAttack += HandleAttackStartInput;
            _inputService.OnChant += HandleChantStartInput;
            _inputService.OnNumSwitch += HandleNumSwitchInput;
            _inputService.OnInteract += HandleInteractInput;
            Debug.Log("已訂閱輸入事件");
        }
        private void UnsubscribeInputEvents()
        {
            _inputService.OnMove -= HandleMoveInput;
            _inputService.OnStop -= HandleStopInput;
            _inputService.OnJump -= HandleJumpInput;
            _inputService.OnRun -= HandleRunInput;
            _inputService.OnDodge -= HandleDodgeInput;
            _inputService.OnAimStart -= HandleAimStartInput;
            _inputService.OnAimEnd -= HandleAimEndInput;
            _inputService.OnFireStart -= HandleFireStartInput;
            _inputService.OnFireEnd -= HandleFireEndInput;
            _inputService.OnAttack -= HandleAttackStartInput;
            _inputService.OnChant -= HandleChantStartInput;
            _inputService.OnNumSwitch -= HandleNumSwitchInput;
            _inputService.OnInteract -= HandleInteractInput;
            Debug.Log("已取消訂閱輸入事件");
        }
        private StateMachine stateMachine => this;
        private void HandleMoveInput() => stateMachine.CurrentState?.OnMoveInput();
        private void HandleStopInput() => stateMachine.CurrentState?.OnStopInput();
        private void HandleJumpInput() => stateMachine.CurrentState?.OnJumpInput();
        private void HandleRunInput() => stateMachine.CurrentState?.OnRunInput();
        private void HandleDodgeInput() => stateMachine.CurrentState?.OnDodgeInput();
        private void HandleAimStartInput() => stateMachine.CurrentState?.OnAimStartInput();
        private void HandleAimEndInput() => stateMachine.CurrentState?.OnAimEndInput();
        private void HandleFireStartInput() => stateMachine.CurrentState?.OnFireStartInput();
        private void HandleFireEndInput() => stateMachine.CurrentState?.OnFireEndInput();
        private void HandleAttackStartInput() => stateMachine.CurrentState?.OnAttackInput();
        private void HandleChantStartInput() => stateMachine.CurrentState?.OnChantInput();
        private void HandleNumSwitchInput() => stateMachine.CurrentState?.OnNumSwitchInput();
        private void HandleInteractInput() => stateMachine.CurrentState?.OnInteractInput();
        // ========================================================================
        //private void HandleTakeDamageAPI(PlayerDamagedEvent evt) => stateMachine.CurrentState?.OnTakeDamge(evt);
        private void SubscribeAPIEvents()
        {
            // 取消訂閱 API 事件
            //_eventBus.Subscribe<PlayerDamagedEvent>(OnTakeDamage);
            //_eventBus.Subscribe<PlayerDamagedEvent>(HandleTakeDamageAPI);
        }
        private void UnsubscribeAPIEvents()
        {
            // 取消訂閱 API 事件
            //_eventBus.Unsubscribe<PlayerDamagedEvent>(OnTakeDamage);
            //// 例如：_eventBus.Unsubscribe<TakeDamageEvent>(HandleTakeDamageAPI);
        }
        //private void OnTakeDamage(PlayerDamagedEvent evt)
        //{
        //    return;
        //    TakeDamage(evt.Damage);
        //}
        ///// <summary>
        ///// 受到傷害
        ///// </summary>
        //public void TakeDamage(int damage)
        //{
        //    if (stateMachine.IsInState<DodgingState>()) return;
        //    _context.CurrentHealth -= damage;

        //    if (_context.CurrentHealth <= 0)
        //    {
        //        _context.CurrentHealth = 0;
        //        stateMachine.ChangeState<DyingState>(force: true);
        //    }
        //    else
        //    {
        //        if (stateMachine.CurrentState.CanTransitionTo(typeof(HitState)))
        //        {
        //            stateMachine.ChangeState<HitState>();
        //        }
        //    }


        //}
    }

    // ========================================================================
    // Core Interfaces（核心接口）
    // ========================================================================

    /// <summary>
    /// 狀態基礎接口
    /// </summary>
    public interface IState
    {
        string StateName { get; }
        IState ParentState { get; }
    }

    /// <summary>
    /// 可進入/退出的狀態
    /// </summary>
    public interface IEnterable
    {
        void Enter();
        void Exit();
    }


}