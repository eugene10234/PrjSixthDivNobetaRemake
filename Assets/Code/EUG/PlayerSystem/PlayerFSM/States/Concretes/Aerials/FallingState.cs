using Nobeta.EUG.FSM.Core;
using System;
using UnityEngine;
// ========================================================================
// FallingState - 下落狀態
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    public class FallingState : AerialState
    {
        public FallingState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            PlayAnimation("Fall",0.15f);
        }
    }
}
