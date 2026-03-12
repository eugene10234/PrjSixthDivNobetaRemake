using Nobeta.EUG.Interfaces;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nobeta.EUG.Services
{
    public class PlayerInputService : IPlayerInputService, IDisposable
    {

        #region PlayerInputService_Construct_Dipose
        private readonly InputSystem_Actions _actions;
        public PlayerInputService()
        {
            Debug.Log("PlayerInputService Initialize");
            _actions = new InputSystem_Actions();
            _actions.Player.Enable();
            AssignInputEvents();
        }
        public void Dispose()
        {
            Debug.Log("PlayerInputService Dispose");
            //解除引用：防止記憶體洩漏 (GC 才能回收這台 Service)
            UnassignInputEvents();
            //停用輸入：防止後台持續輪詢與邏輯干擾
            _actions.Player.Disable();
            //釋放資源：彻底銷毀這個 Action 實例 (如果這個 Actions 是由該 Service new 出來的)
            // _actions.Dispose();
        }
        #endregion
        //=========================================================
        public InputSystem_Actions Actions => _actions;

        // 暴露給外部讀取的狀態
        public Vector2 MoveInput { get; private set; }
        public Vector2 MousePosOffset { get; private set; }
        public bool IsJumpPressed { get; private set; }
        public bool IsRunPressed { get; private set; }
        public bool IsAimPressed { get; private set; }
        public bool IsFirePressed { get; private set; }
        public bool IsCusorSwitchPressed { get; private set; }

        // 提供事件訂閱
        public event Action OnMove;
        public event Action OnStop;
        public event Action OnMouseMove;
        public event Action OnMouseStop;
        public event Action OnRun;
        public event Action OnJump;
        public event Action OnDodge;
        public event Action OnAimStart;
        public event Action OnAimEnd;
        public event Action OnFireStart;
        //鼠標切換事件（如果需要的話）
        public event Action OnCusorSwitchStart;
        public event Action OnCusorSwitchEnd;
        //=========================================================

        private void OnMovementPerformed(InputAction.CallbackContext ctx)
        {
            MoveInput = ctx.ReadValue<Vector2>();
            OnMove?.Invoke();
        }
        private void OnMovementCanceled(InputAction.CallbackContext _)
        {
            MoveInput = Vector2.zero;
            OnStop?.Invoke();
        }
        private void OnMousePosPerformed(InputAction.CallbackContext ctx)
        {
            if (IsCusorSwitchPressed) MousePosOffset = Vector2.zero;
            else MousePosOffset = ctx.ReadValue<Vector2>();
            OnMouseMove?.Invoke();
        }
        private void OnMousePosCanceled(InputAction.CallbackContext _)
        {
            MousePosOffset = Vector2.zero;
            OnMouseStop?.Invoke();
        }
        private void RunPerformed(InputAction.CallbackContext _)
        {
            IsRunPressed = true;
            OnRun?.Invoke();
        }
        private void RunCanceled(InputAction.CallbackContext _)
        {
            IsRunPressed = false;
        }
        private void JumpPerformed(InputAction.CallbackContext _)
        {
            IsJumpPressed = true;
            OnJump?.Invoke();
        }
        private void JumpCanceled(InputAction.CallbackContext _)
        {
            IsJumpPressed = false;
        }
        private void DodgePerformed(InputAction.CallbackContext _)
        {
            OnDodge?.Invoke();
        }
        private void AimPerformed(InputAction.CallbackContext _)
        {
            IsAimPressed = true;
            OnAimStart?.Invoke();
        }
        private void AimCanceled(InputAction.CallbackContext _)
        {
            IsAimPressed = false;
            OnAimEnd?.Invoke();
        }
        private void FirePerformed(InputAction.CallbackContext _)
        {
            if (IsCusorSwitchPressed) return;
            IsFirePressed = true;
            OnFireStart?.Invoke();
        }
        private void FireCanceled(InputAction.CallbackContext _)
        {
            IsFirePressed = false;
        }
        private void CusorSwitchPerformed(InputAction.CallbackContext _)
        {
            IsCusorSwitchPressed = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        private void CusorSwitchCanceled(InputAction.CallbackContext _)
        {
            IsCusorSwitchPressed = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        //=========================================================
        #region InputEvents_Assign_Unassign
        private void AssignInputEvents()
        {
            _actions.Player.Movement.performed += OnMovementPerformed;
            _actions.Player.Movement.canceled += OnMovementCanceled;
            _actions.Player.MousePos.performed += OnMousePosPerformed;
            _actions.Player.MousePos.canceled += OnMousePosCanceled;
            _actions.Player.Run.performed += RunPerformed;
            _actions.Player.Run.canceled += RunCanceled;
            _actions.Player.Jump.performed += JumpPerformed;
            _actions.Player.Jump.canceled += JumpCanceled;
            _actions.Player.Dodge.performed += DodgePerformed;
            _actions.Player.Aim.performed += AimPerformed;
            _actions.Player.Aim.canceled += AimCanceled;
            _actions.Player.Fire.performed += FirePerformed;
            _actions.Player.Fire.canceled += FireCanceled;
            _actions.Player.CursorSwitch.performed += CusorSwitchPerformed;
            _actions.Player.CursorSwitch.canceled += CusorSwitchCanceled;

        }
        private void UnassignInputEvents()
        {
            _actions.Player.Movement.performed -= OnMovementPerformed;
            _actions.Player.Movement.canceled -= OnMovementCanceled;
            _actions.Player.Run.performed -= RunPerformed;
            _actions.Player.Run.canceled -= RunCanceled;
            _actions.Player.Jump.performed -= JumpPerformed;
            _actions.Player.Jump.canceled -= JumpCanceled;
            _actions.Player.Dodge.performed -= DodgePerformed;
            _actions.Player.Aim.performed -= AimPerformed;
            _actions.Player.Aim.canceled -= AimCanceled;
            _actions.Player.Fire.performed -= FirePerformed;
            _actions.Player.Fire.canceled -= FireCanceled;
            _actions.Player.CursorSwitch.performed -= CusorSwitchPerformed;
            _actions.Player.CursorSwitch.canceled -= CusorSwitchCanceled;
        }
        #endregion

    }
}
