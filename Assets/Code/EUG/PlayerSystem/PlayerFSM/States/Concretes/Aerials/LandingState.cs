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
        //private float landingTimer;
        private bool landingTimerLock;
        //public void LandingTimerLocker(bool a) => landingTimerLock = a;
        private const float LANDING_DURATION = 0.05f;
        private Vector3 _currentVelocity;

        public LandingState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            //landingTimer = LANDING_DURATION;
            ctx.TimeService.TaskDelayLock(LANDING_DURATION, (b) => landingTimerLock = b);
            _currentVelocity = ctx.MovementService.CurrentVelocity;
            ctx.MovementService.ResetVelocity();
            //ctx.MovementService.remainVelocity(0.8f);
            PlayAnimation("Land");
        }

        public override void Update()
        {
            //landingTimer -= Time.deltaTime;

            //if (landingTimer <= 0)
            if (!landingTimerLock)
            {


                Type LastGroundedState = stateMachine.GetLastGroundedState();
                Debug.Log("LastGroundedState" + LastGroundedState);
                //落地硬直結束
                stateMachine.ChangeStateByParent<GroundedState>(LastGroundedState);
                if (ctx.InputService.MoveInput.magnitude > 0.1f)
                {
                    stateMachine.ChangeState<WalkingState>();

                }
                else
                {
                    stateMachine.ChangeState<IdleState>();
                }
            }

        }
        public override void Exit()
        {
            base.Exit();
            ExitFlagCheck();
        }
        private void ExitFlagCheck()
        {
            if (ctx.CurrentBulletConfig.ChantFlag)
            {
                ctx.ChantContinue();
            }
        }
        public override void OnFireStartInput()
        {            
        }
        public override void OnFireEndInput()
        {       
        }
        public override bool CanTransitionTo(Type stateType)
        {
            // 落地硬直期間只能被打斷為受擊/死亡
            if (stateType == typeof(HitState) || stateType == typeof(DyingState))
                return true;

            return !landingTimerLock;// landingTimer <= 0;
        }
    }
}
