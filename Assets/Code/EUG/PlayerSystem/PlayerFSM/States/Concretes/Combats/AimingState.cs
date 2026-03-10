
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Services;
using System;
using UnityEngine;


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
            PlayAnimation("AimingWalk");

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
           
            AnimatorSetting();
        }

        public void FixedUpdate()
        {
            // 瞄準時可以緩慢移動
            _movement = ctx.InputService.MoveInput;

            if (_movement.magnitude > 0.1f)
            {
                Vector3 direction = ctx.CameraService.GetCameraRelativeDirection(_movement);

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
            Vector3 Velocity = ctx.MovementService.CurrentVelocity;
            ctx.AnimationService.SetFloat("xVelocity", Velocity.x);
            ctx.AnimationService.SetFloat("zVelocity", Velocity.z);

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
            var offsetPosition = ctx.Transform.position;
            offsetPosition+= ctx.CameraService.GetCameraForward() * 1.5f; // 在玩家前方生成子彈
            Debug.LogWarning("發射武器"+ ctx.Transform.position);
            //context.EventBus.Publish(new WeaponFiredEvent
            //{
            //    BulletType = BulletType.Fire,
            //    Position = offsetPosition,
            //    Forward = context.Transform.forward

            //});
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
