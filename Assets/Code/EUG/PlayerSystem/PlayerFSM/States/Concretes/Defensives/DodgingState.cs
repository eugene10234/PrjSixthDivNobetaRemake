using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.Interfaces;
using System;
using UnityEngine;
// ========================================================================
// DodgingState - 閃避狀態
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    public class DodgingState : BaseState, IUpdatable, IFixedUpdatable
    {
        protected float stateStartStamp;
        private float dodgeTimer;
        private Vector3 dodgeDirection;
        private bool dodgeDirCheck;
        public DodgingState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
            stateStartStamp =Time.time;
        }

        public override void Enter()
        {
            base.Enter();
            ctx.ConsumeStamina(ctx.Config.dodgeStaminaCost);
            dodgeDirCheck = true;
            // 設置閃避方向
            Vector2 input = ctx.InputService.MoveInput;

            if (input.magnitude > 0.1f)
            {
                dodgeDirection = ctx.CameraService.GetCameraRelativeDirection(input);
                PlayAnimation("Dodge_F", 0.1f);
            }
            else
            {
                dodgeDirCheck = false;
                dodgeDirection = -ctx.Transform.forward;
                PlayAnimation("Dodge_B");
            }

            // 應用閃避移動 閃避期間維持速度
            //ApplyDodge();
            ctx.TimeService.DelayAction(ApplyDodge, 0.15f).Forget();
            ctx.MovementService.ReduceGravity(0.4f);
            PlaySound("Dodge");
            // 消耗閃避次數
            ctx.DodgeCharges--;
            ctx.LastDodgeTime = Time.time;

            dodgeTimer = ctx.Config.dodgeDuration;


        }

        private void ApplyDodge()
        {
            
            ctx.MovementService.ApplyDodge(
                dodgeDirection,
                ctx.Config.dodgeDistance,
                ctx.Config.dodgeDuration
            );
        }

        public void Update()
        {
            dodgeTimer -= Time.deltaTime;

            if (dodgeTimer <= 0)
            {
                // 閃避結束
                if (ctx.IsGrounded)
                {
                    Type LastGroundedState = stateMachine.GetLastGroundedState();
                    //Debug.Log("LastGroundedState" + LastGroundedState);
                    stateMachine.ChangeStateByParent<GroundedState>(LastGroundedState);
                }
                else
                {
                    stateMachine.ChangeState<FallingState>();
                }
            }
        }

        public void FixedUpdate()
        {
            // 閃避期間維持速度
            if (dodgeDirCheck)
            {
                ctx.MovementService.RotateTowards(
                    dodgeDirection,
                    ctx.Config.rotationSpeed
                    );
            }


            ctx.MovementService.ExecuteMovement();
        }

        public override bool CanTransitionTo(Type stateType)
        {
            // 閃避期間無敵，只能轉到死亡
            if (stateType == typeof(DyingState))
                return true;

            return dodgeTimer <= 0;
        }
    }

}
