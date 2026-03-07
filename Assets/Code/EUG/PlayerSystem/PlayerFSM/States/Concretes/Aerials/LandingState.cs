using Nobeta.EUG.FSM.Core;
using System;
using System.Linq;
using UnityEngine;
// ========================================================================
// LandingState - 落地狀態
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    public class LandingState : GroundedState
    {
        private float landingTimer;
        private const float LANDING_DURATION = 0.1f;
        private Vector3 _currentVelocity;

        public LandingState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            landingTimer = LANDING_DURATION;
            _currentVelocity = ctx.MovementService.CurrentVelocity;
            ctx.MovementService.ResetVelocity();
            //context.MovementService.remainVelocity(0.8f);
            PlayAnimation("Land");
        }

        public override void Update()
        {
            landingTimer -= Time.deltaTime;

            if (landingTimer <= 0)
            {
                Type LastGroundedState = stateMachine.GetLastGroundedState();
                //Debug.Log("LastGroundedState" + LastGroundedState);
                // 落地硬直結束
                stateMachine.ChangeStateByParent<GroundedState>(LastGroundedState);
                //if (context.InputService.MoveInput.magnitude > 0.1f)
                //{
                //    stateMachine.ChangeState<WalkingState>();

                //}
                //else
                //{
                //    stateMachine.ChangeState<IdleState>();
                //}
            }
        }

        public override bool CanTransitionTo(System.Type stateType)
        {
            // 落地硬直期間只能被打斷為受擊/死亡
            if (stateType == typeof(HitState) || stateType == typeof(DyingState))
                return true;

            return landingTimer <= 0;
        }
    }
}
