// ============================================================================
// Concrete States - 具體狀態實現（使用服務）
// ============================================================================

using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.Interfaces;
using UnityEngine;

namespace Nobeta.EUG.FSM.States
{






    // ========================================================================
    // HitState 和 DyingState - 簡化版
    // ========================================================================

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

            PlayAnimation("Hit");
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

    public class DyingState : BaseState
    {
        public DyingState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            ctx.MovementService.ResetVelocity();

            PlayAnimation("Die");
            PlaySound("Die");
        }
    }
}