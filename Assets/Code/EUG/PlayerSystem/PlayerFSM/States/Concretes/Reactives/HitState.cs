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

    public class HitState : BaseState, IUpdatable
    {
        private float hitTimer;

        public HitState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            hitTimer = ctx.Config.hitStunDuration;
            ctx.MovementService.ResetVelocity();

            PlayAnimation("Head_Hit_R");
            PlaySound("Hit");
        }

        public void Update()
        {
            hitTimer -= Time.deltaTime;

            if (hitTimer <= 0)
            {
                stateMachine.ChangeState<IdleState>();
            }
        }
    }
}