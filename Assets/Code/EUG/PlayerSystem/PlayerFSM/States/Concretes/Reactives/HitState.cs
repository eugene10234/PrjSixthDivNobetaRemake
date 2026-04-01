// ============================================================================
// Concrete States - 具體狀態實現（使用服務）
// ============================================================================

using Nobeta.EUG.Events;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.Interfaces;
using UnityEngine;
    // ========================================================================
    // HitState  簡化版
    // ========================================================================

namespace Nobeta.EUG.FSM.States
{

    public class HitState : ReactiveState, IUpdatable
    {
        private float hitTimer;

        public HitState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
            reactionDuration = ctx.Config.hitStunDuration;
        }

        public override void Enter()
        {
            base.Enter();
            //ctx.MovementService.ResetVelocity();
            PlayAnimation("Head_Hit_R");
            PlaySound("Hit");
        }

        protected override void OnReactionComplete()
        {
            hitTimer -= Time.deltaTime;

            if (hitTimer <= 0)
            {
                stateMachine.ChangeState<IdleState>();
            }
        }
    }
}