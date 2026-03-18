
using System;
using Nobeta.EUG.AANEW.MBbulletsystems;
using Nobeta.EUG.BulletSystem.OOP.UnityBridge;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Services;
using Nobeta.EUG.Interfaces;
using UnityEngine;
using VContainer;


namespace Nobeta.EUG.FSM.States
{
    // ========================================================================
    // AimingState - 瞄準狀態
    // ========================================================================

    /// <summary>
    /// 瞄準狀態
    /// 玩家進入瞄準模式，準備施法或攻擊
    /// </summary>
    public class AimingState : CombatState, IFixedUpdatable
    {
        private Type _speedType;
        private float _targetSpeed;
        private Vector2 _movement;
        DebugServiceMb _debugServiceMb;
        private float _turnAngle;
        [Inject] IEventBus _eventBus;
        public AimingState(StateContext context, StateMachine stateMachine, DebugServiceMb debugServiceMb)
            : base(context, stateMachine)
        {
            _debugServiceMb = debugServiceMb;
        }

        public override void Enter()
        {
            base.Enter();
            _speedType = stateMachine.GetLastGroundedState();
            SetTargetSpeed(_speedType);
            PlayAnimation("AimingWalk2",0.1f);

            // 降低移動速度
            ctx.MovementService.StopMovement(ctx.Config.acceleration * 2f);

            // 發布事件（通知攝影機切換）
            //PublishEvent(new PlayerAimingStartedEvent());
            //_debugServiceMb.AddGiz(() =>
            //{
            //    Gizmos.color = Color.red; // 甚至可以順便設顏色
            //    Gizmos.DrawLine(context.Transform.position,
            //        context.Transform.position + context.CameraService.GetCameraRelativeDirection(context.InputService.MoveInput) * 2f);
            //    //_debugServiceMb.ClearGiz();
            //});
            //_debugServiceMb.AddGiz(() =>
            //{
            //    Gizmos.color = Color.blue; // 甚至可以順便設顏色
            //    Gizmos.DrawLine(context.Transform.position,
            //        context.Transform.position + context.Transform.forward);
            //    //_debugServiceMb.ClearGiz();
            //});
            Vector3 currentVelocity = ctx.MovementService.CurrentVelocity;
            //_debugServiceMb.Add(DrawDebugInfo);
        }
        private void DrawDebugInfo()
        {
            GUILayout.Box("=== AngleTest ===");
            GUILayout.Label("<b>turnAngle:</b>");
            GUILayout.Label($"turnAngle: {_turnAngle}");
        }
        public override void Exit()
        {
            base.Exit();

            // 發布事件
            //PublishEvent(new PlayerAimingEndedEvent());
        }

            
            //if (Input.GetMouseButtonDown(0))
            //{
            //    stateMachine.ChangeState<ChargingState>();
            //    return;
            //}


        public override void Update()
        {
            base.Update();
            //if (Input.GetMouseButtonDown(0))
            //{
            //    //Debug.Log("開始攻擊");
            //    //return;
            //}
            // 持續面向瞄準方向
            Vector3 aimDirection = GetAimDirection();
            ctx.MovementService.RotateTowards(aimDirection, ctx.Config.rotationSpeed);
            
            Vector3 movement = ctx.CameraService.GetCameraRelativeDirection(ctx.InputService.MoveInput);
            _turnAngle = Vector3.SignedAngle(ctx.Transform.forward, movement, Vector3.up);           
            SetTargetSpeed(_speedType);
            AnimatorSetting();
        }

        public void FixedUpdate()
        {
            //Debug.LogWarning($"111111111111111111111111111:{DateTime.Now}");
            // 瞄準時可以緩慢移動
            _movement = ctx.InputService.MoveInput;
            //Debug.LogWarning($"MoveInput: {_movement}:{DateTime.Now}");
            if(!ctx.InputService.IsAimPressed) stateMachine.ChangeStateByParent<GroundedState>(_speedType);
            if (_movement.magnitude > 0.1f)
            {
               // Debug.LogWarning($"222222222222222222:{DateTime.Now}");
                if (typeof(IdleState) == _speedType) _speedType = typeof(WalkingState);
                Vector3 direction = ctx.CameraService.GetCameraRelativeDirection(_movement);
                //Debug.LogWarning($"_targetSpeed==={_targetSpeed}:{DateTime.Now}");
                // 使用較低的速度
                ctx.MovementService.ApplyGroundMovement(
                    direction,
                    _targetSpeed * 0.5f,  // 瞄準時速度減半
                    ctx.Config.acceleration
                );
            }
            else
            {

                // 沒有輸入時停止移動
                ctx.MovementService.StopMovement(ctx.Config.acceleration * 2f);
            }
            // 應用重力
            ctx.MovementService.ApplyGravity(ctx.Config.gravity);

            // 執行移動
            ctx.MovementService.ExecuteMovement();
        }
        private void AnimatorSetting()
        {
            Vector2 dir = ctx.InputService.MoveInput;
            ctx.AnimationService.SetFloat("xVelocity", dir.x);
            ctx.AnimationService.SetFloat("zVelocity", dir.y);

            //bool playRunAnimation = isRunning & movementDirection.magnitude > 0;
            //animator.SetBool("isRunning", playRunAnimation);
        }
        // 輸入事件處理
        public override void OnJumpInput()
        {
            // 瞄準時不能跳躍
            //Debug.LogWarning("OnJumpInput");
        }
        public override void OnFireStartInput()
        {
            //var offsetPosition = ctx.Transform.position;
            //offsetPosition+= ctx.CameraService.GetCameraForward() * 1.5f; // 在玩家前方生成子彈
            //Debug.LogWarning("發射武器"+ ctx.Transform.position);
            //ctx.EventBus.Publish(new WeaponFiredEvent
            //{
            //    BulletType = BulletType.Fire,
            //    Position = offsetPosition,
            //    Forward = context.Transform.forward

            //});
            _eventBus.Publish(new WeaponFiredEvent2{});
        }
        public override void OnDodgeInput()
        {
            if (ctx.CanDodge)
            {
                stateMachine.ChangeState<DodgingState>();
            }
        }
        public override void OnStopInput()
        {
            _speedType = typeof(IdleState);
            SetTargetSpeed(_speedType);
        }
        public override void OnMoveInput()
        {
            if (_movement.magnitude < 0.1f)
            {
                _speedType = typeof(WalkingState);
                SetTargetSpeed(_speedType);
            }
        }
        public override void OnRunInput()
        {
            if (_movement.magnitude > 0.1f)
            {
                _speedType = typeof(RunningState);
                SetTargetSpeed(_speedType);
            }
        }
        public override void OnAimEndInput()
        {
            
            if (ctx.IsGrounded)
            {
                stateMachine.ChangeStateByParent<GroundedState>(_speedType);
            }
            else
            {
                stateMachine.ChangeState<FallingState>();
            }
        }
        private void SetTargetSpeed(Type speedType)
        {
            if (typeof(IdleState) == speedType)
            {
                _targetSpeed = 0f;
            }
            else if (typeof(WalkingState) == speedType)
            {
                _targetSpeed = ctx.Config.walkSpeed;
            }
            else if (typeof(RunningState) == speedType)
            {
                _targetSpeed = ctx.Config.runSpeed;
            }
        }
    }
}
