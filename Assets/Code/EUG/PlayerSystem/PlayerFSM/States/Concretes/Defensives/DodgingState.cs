using Nobeta.EUG.FSM.Core;
using System;
using UnityEngine;
// ========================================================================
// DodgingState - 閃避狀態
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    public class DodgingState : BaseState, IUpdatable, IFixedUpdatable
    {
        private float dodgeTimer;
        private Vector3 dodgeDirection;
        private bool dodgeDirCheck;
        public DodgingState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            dodgeDirCheck = true;
            // 設置閃避方向
            Vector2 input = context.InputService.MoveInput;

            if (input.magnitude > 0.1f)
            {
                dodgeDirection = context.CameraService.GetCameraRelativeDirection(input);
                PlayAnimation("Dodge_F");
            }
            else
            {
                dodgeDirCheck = false;
                dodgeDirection = -context.Transform.forward;
                PlayAnimation("Dodge_B");
            }

            // 應用閃避移動 閃避期間維持速度
            context.MovementService.ApplyDodge(
                dodgeDirection,
                context.Config.dodgeDistance,
                context.Config.dodgeDuration
            );
   
            PlaySound("Dodge");
            // 消耗閃避次數
            context.DodgeCharges--;
            context.LastDodgeTime = Time.time;

            dodgeTimer = context.Config.dodgeDuration;


        }

        public void Update()
        {
            dodgeTimer -= Time.deltaTime;

            if (dodgeTimer <= 0)
            {
                // 閃避結束
                if (context.IsGrounded)
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
                context.MovementService.RotateTowards(
                    dodgeDirection,
                    context.Config.rotationSpeed
                    );
            }


            context.MovementService.ExecuteMovement();
        }

        public override bool CanTransitionTo(System.Type stateType)
        {
            // 閃避期間無敵，只能轉到死亡
            if (stateType == typeof(DyingState))
                return true;

            return dodgeTimer <= 0;
        }
    }

}
