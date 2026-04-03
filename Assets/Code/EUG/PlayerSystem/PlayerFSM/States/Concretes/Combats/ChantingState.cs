
using Nobeta.EUG.AANEW.MBbulletsystems;
using Nobeta.EUG.BulletSystem.OOP.UnityBridge;
using Nobeta.EUG.Events;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Services;
using Nobeta.EUG.Interfaces;
using System;
using UnityEngine;
using VContainer;
// ========================================================================
// ChantingState - 詠唱狀態
// ========================================================================

namespace Nobeta.EUG.FSM.States
{

    /// <summary>
    /// 瞄準狀態
    /// 玩家進入瞄準模式，準備施法或攻擊
    /// </summary>
    public class ChantingState : CombatState, IFixedUpdatable
    {
        private Type _speedType;
        private float _targetSpeed;
        private Vector2 _movement;
        DebugServiceMb _debugServiceMb;
        private float _turnAngle;
        [Inject] IEventBus _eventBus;
        private float _idleTimeStamp;
        private bool _isAimAnimate;

        public ChantingState(StateContext context, StateMachine stateMachine, DebugServiceMb debugServiceMb)
            : base(context, stateMachine)
        {
            _debugServiceMb = debugServiceMb;
            _idleTimeStamp = Time.time;
        }

        public override void Enter()
        {
            base.Enter();
            ctx.CommandChant();
            PublishEvent(new RigChantStartEvent());
            _speedType = stateMachine.GetLastGroundedState();
            SetTargetSpeed(_speedType);
            if (ctx.InputService.IsAimPressed)
            {
                PlayAnimation("AimingWalk2", 0.1f);
                _isAimAnimate = true;
            }
            else
            {
                PlayAnimation("MovementState", 0.1f);
                _isAimAnimate = false;
            }



            // 降低移動速度
            ctx.MovementService.StopMovement(ctx.Config.acceleration * 2f);

            Vector3 currentVelocity = ctx.MovementService.CurrentVelocity;
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
            PublishEvent(new RigChantEndEvent());
        }


        public override void Update()
        {
            base.Update();
            //if (Input.GetMouseButtonDown(0))
            //{
            //    //Debug.Log("開始攻擊");
            //    //return;
            //}
            // 持續面向瞄準方向
            Vector3 direction;
            if (ctx.InputService.IsAimPressed)
                direction = GetAimDirection();
            else
                direction = GetMoveDirection();
            ctx.MovementService.RotateTowards(direction, ctx.Config.rotationSpeed);

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
            //if(!ctx.InputService.IsAimPressed) stateMachine.ChangeStateByParent<GroundedState>(_speedType);
            if (_movement.sqrMagnitude > 0.01f)
            {

                if (typeof(IdleState) == _speedType) _speedType = typeof(WalkingState);
                Vector3 direction = ctx.CameraService.GetCameraRelativeDirection(_movement);
                //Debug.LogWarning($"_targetSpeed==={_targetSpeed}:{DateTime.Now}");
                // 使用較低的速度
                ctx.MovementService.ApplyGroundMovement(
                    direction,
                    _targetSpeed * 0.5f * SpeedStartFilter(),  // 瞄準時速度減半
                    ctx.Config.acceleration
                );
            }
            else
            {
                _idleTimeStamp = Time.time;
                // 沒有輸入時停止移動
                ctx.MovementService.StopMovement(ctx.Config.acceleration * 2f);
            }
            // 應用重力
            ctx.MovementService.ApplyGravity(ctx.Config.gravity);

            // 執行移動
            ctx.MovementService.ExecuteMovement();
        }
        private float SpeedStartFilter()
        {
            if (Time.time - _idleTimeStamp < 0.15f)
            {
                return 0.15f;
            }
            return 1f;
        }
        private void AnimatorSetting()
        {
            if (ctx.InputService.IsAimPressed)
            {
                if (!_isAimAnimate)
                {
                    PlayAnimation("AimingWalk2", 0.1f);
                    _isAimAnimate = true;
                }
                Vector2 dir = ctx.InputService.MoveInput;
                ctx.AnimationService.SetFloat("xVelocity", dir.x);
                ctx.AnimationService.SetFloat("zVelocity", dir.y);
            }
            else
            {
                if (_isAimAnimate)
                {
                    PlayAnimation("MovementState", 0.1f);
                    _isAimAnimate = false;
                }
                var customAniSpeed = GetAniSpeed(ctx.CurrentSpeed);
                ctx.AnimationService.SetFloat("Speed", customAniSpeed);
                ctx.AnimationService.SetFloat("Rotation", _turnAngle / 32f);
            }


            //Vector2 dir = ctx.InputService.MoveInput;
            //ctx.AnimationService.SetFloat("xVelocity", dir.x);
            //ctx.AnimationService.SetFloat("zVelocity", dir.y);

            //bool playRunAnimation = isRunning & movementDirection.magnitude > 0;
            //animator.SetBool("isRunning", playRunAnimation);
        }
        private float GetAniSpeed(float speed)
        {
            // 確保輸入在 0 到 10 之間
            float clampSpeed = Mathf.Clamp(speed, 0f, ctx.Config.runSpeed);

            if (_movement.sqrMagnitude > 0.01f)
            {
                float t = clampSpeed / (ctx.Config.runSpeed * 0.5f);

                return Mathf.Lerp(0, 5, t);
            }
            else
            {
                return -4f;
            }
        }
        // 輸入事件處理
        public override void OnJumpInput()
        {
            // 瞄準時不能跳躍
            //Debug.LogWarning("OnJumpInput");
        }
        //public override void DetectionHandler()
        //{ 
        //        base.DetectionHandler();
        //    //    if (ctx.InputService.IsJumpPressed)
        //    //    {
        //    //        //Debug.LogWarning("OnJumpInput");
        //    //        return;
        //    //}
        //}
        //public override void OnFireStartInput()
        //{
        //    //var offsetPosition = ctx.Transform.position;
        //    //offsetPosition+= ctx.CameraService.GetCameraForward() * 1.5f; // 在玩家前方生成子彈
        //    //Debug.LogWarning("發射武器"+ ctx.Transform.position);
        //    //ctx.EventBus.Publish(new WeaponFiredEvent
        //    //{
        //    //    BulletType = BulletType.Fire,
        //    //    Position = offsetPosition,
        //    //    Forward = context.Transform.forward

        //    //});
        //    //_eventBus.Publish(new WeaponFiredEvent2{});
        //    //ctx.FireMagic();
        //}
        //public override void OnFireEndInput()
        //{
        //    //var offsetPosition = ctx.Transform.position;
        //    //offsetPosition+= ctx.CameraService.GetCameraForward() * 1.5f; // 在玩家前方生成子彈
        //    //Debug.LogWarning("發射武器"+ ctx.Transform.position);
        //    //ctx.EventBus.Publish(new WeaponFiredEvent
        //    //{
        //    //    BulletType = BulletType.Fire,
        //    //    Position = offsetPosition,
        //    //    Forward = context.Transform.forward

        //    //});
        //    //_eventBus.Publish(new WeaponFiredEvent2{});
        //    //ctx.FireMagic();
        //}
        //public override void OnDodgeInput()
        //{
        //    if (ctx.CanDodge)
        //    {
        //        stateMachine.ChangeState<DodgingState>();
        //    }
        //}

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
        public override void OnChantInput()
        {
            ctx.ChantKill();
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
