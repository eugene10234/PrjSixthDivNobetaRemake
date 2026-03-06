// ============================================================================
// PlayerController - 玩家控制器（完整事件驅動版）
// 純協調者，不包含任何實現邏輯
// ============================================================================


using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.FSM.States;
using System;
using UnityEngine;
using VContainer;
using static UnityEngine.EventSystems.EventTrigger;

namespace Nobeta.EUG.FSM
{
    /// <summary>
    /// 玩家控制器
    /// 
    /// 職責：
    /// - 協調狀態機和服務
    /// - 訂閱輸入事件
    /// - 提供公共 API
    /// 
    /// 不負責：
    /// - 移動計算
    /// - 輸入處理
    /// - 物理應用
    /// - 狀態邏輯
    /// </summary>
    //[RequireComponent(typeof(CharacterController))]
    public class mbPlayerController : MonoBehaviour
    {
        //#region Dependencies（依賴注入）

        //[Inject] private StateMachine stateMachine;
        //[Inject] private StateContext context;
        //[Inject] private IPlayerInputService inputService;
        //[Inject] private IDetectionService groundDetection;
        //[Inject] private IEventBus eventBus;
        //private bool showDebugInfo;

        ////[Inject]
        ////public void Construct(
        ////    StateMachine stateMachine,
        ////    StateContext context,
        ////    IInputService inputService,
        ////    IGroundDetectionService groundDetection,
        ////    IEventBus eventBus)
        ////{
        ////    this.stateMachine = stateMachine;
        ////    this.context = context;
        ////    this.inputService = inputService;
        ////    this.groundDetection = groundDetection;
        ////    this.eventBus = eventBus;
        ////}

        //#endregion

        //#region Unity Lifecycle
        //private void Start()
        //{
        //    // Cursor.lockState = CursorLockMode.Locked;
        //    //Cursor.visible = false;
        //    showDebugInfo = true;
        //    // 訂閱輸入事件
        //    Debug.LogWarning("mbPlayerControllerStart");
        //    SubscribeToInputEvents();

        //    // 設置初始狀態
        //    stateMachine.SetInitialState<IdleState>();

        //    Debug.Log("PlayerController 初始化完成（事件驅動模式）");
        //}

        //private void Update()
        //{
        //    // 更新地面檢測
        //    groundDetection.UpdateGroundDetection();

        //    // 更新狀態機
        //    stateMachine.Update();

        //    // 檢查全局狀態轉換
        //    CheckGlobalTransitions();
        //}

        //private void FixedUpdate()
        //{
        //    // 固定更新（物理）
        //    stateMachine.FixedUpdate();
        //}

        //private void OnDestroy()
        //{
        //    // 清理事件訂閱
        //    UnsubscribeFromInputEvents();
        //}

        //#endregion

        //#region Input Event Subscription（事件訂閱）

        //private void SubscribeToInputEvents()
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

        //private void UnsubscribeFromInputEvents()
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

        //private void HandleMoveInput() => stateMachine.CurrentState?.OnMoveInput();
        //private void HandleStopInput() => stateMachine.CurrentState?.OnStopInput();
        //private void HandleJumpInput() => stateMachine.CurrentState?.OnJumpInput();
        //private void HandleRunInput() => stateMachine.CurrentState?.OnRunInput();
        //private void HandleDodgeInput() => stateMachine.CurrentState?.OnDodgeInput();
        //private void HandleAimStartInput() => stateMachine.CurrentState?.OnAimStartInput();
        //private void HandleAimEndInput() => stateMachine.CurrentState?.OnAimEndInput();
        //private void HandleFireStartInput() => stateMachine.CurrentState?.OnFireStartInput();
        //#endregion

        //#region Global State Transitions（全局狀態轉換）

        //private void CheckGlobalTransitions()
        //{
        //    // 檢查死亡
        //    if (context.CurrentHealth <= 0 && !stateMachine.IsInState<DyingState>())
        //    {
        //        stateMachine.ChangeState<DyingState>();
        //        return;
        //    }

        //    // 這裡可以添加其他全局轉換邏輯
        //}

        //#endregion

        //#region Public API（公共接口）

        ///// <summary>
        ///// 受到傷害
        ///// </summary>
        //public void TakeDamage(int damage)
        //{
        //    context.CurrentHealth -= damage;

        //    if (context.CurrentHealth <= 0)
        //    {
        //        context.CurrentHealth = 0;
        //        stateMachine.ChangeState<DyingState>();
        //    }
        //    else
        //    {
        //        if (stateMachine.CurrentState.CanTransitionTo(typeof(HitState)))
        //        {
        //            stateMachine.ChangeState<HitState>();
        //        }
        //    }

        //    // 發布事件
        //    eventBus.Publish(new PlayerDamagedEvent
        //    {
        //        Damage = damage,
        //        CurrentHealth = context.CurrentHealth
        //    });
        //}

        ///// <summary>
        ///// 治療
        ///// </summary>
        //public void Heal(int amount)
        //{
        //    context.CurrentHealth = Mathf.Min(
        //        context.CurrentHealth + amount,
        //        context.MaxHealth
        //    );
        //}

        ///// <summary>
        ///// 恢復魔力
        ///// </summary>
        //public void RestoreMana(int amount)
        //{
        //    context.CurrentMana = Mathf.Min(
        //        context.CurrentMana + amount,
        //        context.MaxMana
        //    );
        //}

        ///// <summary>
        ///// 重生
        ///// </summary>
        //public void Respawn()
        //{
        //    context.CurrentHealth = context.MaxHealth;
        //    context.CurrentMana = context.MaxMana;
        //    context.DodgeCharges = context.Config.maxDodgeCharges;
        //    context.MovementService.ResetVelocity();

        //    stateMachine.ChangeState<IdleState>();
        //}

        ///// <summary>
        ///// 獲取當前狀態名稱
        ///// </summary>
        //public string GetCurrentStateName()
        //{
        //    return stateMachine.CurrentState?.StateName ?? "None";
        //}

        ///// <summary>
        ///// 檢查是否在指定狀態
        ///// </summary>
        //public bool IsInState<T>() where T : BaseState
        //{
        //    return stateMachine.IsInState<T>();
        //}

        //#endregion

        // #region Debug（除錯）
        public event Action DebugGUI;
        private void OnGUI()
        {
            DebugGUI?.Invoke();
            //if (true)
            //{
            //    DrawDebugInfo();
            //}

        }

        private void DrawDebugInfo()
        {
            if (!Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Box("=== Player Controller (Event-Driven) ===");

            //GUILayout.Label($"<b>State:</b> {GetCurrentStateName()}");
            //GUILayout.Label($"Health: {context.CurrentHealth}/{context.MaxHealth}");
            //GUILayout.Label($"Mana: {context.CurrentMana}/{context.MaxMana}");
            //GUILayout.Label($"Dodge Charges: {context.DodgeCharges}");
            //GUILayout.Label($"Is Grounded: {context.IsGrounded}");
            //GUILayout.Label($"Speed: {context.CurrentSpeed:F2}");

            //GUILayout.Space(10);

            //GUILayout.Label("<b>Input:</b>");
            //GUILayout.Label($"Move: {context.InputService.MoveInput}");
            //GUILayout.Label($"Jump Pressed: {context.InputService.IsJumpPressed}");
            //GUILayout.Label($"Run Pressed: {context.InputService.IsRunPressed}");

            //GUILayout.Space(10);

            //if (GUILayout.Button("Take Damage (20)"))
            //{
            //    TakeDamage(20);
            //}

            //if (GUILayout.Button("Respawn"))
            //{
            //    Respawn();
            //}

            GUILayout.EndArea();
           
        }
       
        //#endregion
    }

    //// ========================================================================
    //// Game Events（遊戲事件）
    //// ========================================================================

    //public struct PlayerDamagedEvent
    //{
    //    public int Damage;
    //    public int CurrentHealth;
    //}

    //public struct PlayerHealedEvent
    //{
    //    public int Amount;
    //    public int CurrentHealth;
    //}
}