
using System;
using Nobeta.EUG.AANEW.MBbulletsystems;
using Nobeta.EUG.BulletSystem.OOP.UnityBridge;
using Nobeta.EUG.Events;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Services;
using Nobeta.EUG.Interfaces;
using UnityEngine;
using VContainer;
// ========================================================================
// CastingState - 施法狀態
// ========================================================================
namespace Nobeta.EUG.FSM.States
{


    /// <summary>
    /// 施法狀態
    /// 釋放魔法攻擊
    /// </summary>
    public class CastingState : CombatState
    {
        public CastingState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            stateTimer = ctx.Config.castDuration;

            // 執行施法
            //CastSpell();

            PlayAnimation("Cast");
            PlaySound("SpellCast");

            // 停止移動
            ctx.MovementService.ResetVelocity();
        }

        protected override void OnTimerExpired()
        {
            // 施法完成
            if (ctx.InputService.MoveInput.magnitude > 0.1f)
            {
                stateMachine.ChangeState<WalkingState>();
            }
            else
            {
                stateMachine.ChangeState<IdleState>();
            }
        }

        private void CastSpell()
        {
            // 根據蓄力等級計算威力和消耗
            // int spellPower = Mathf.RoundToInt(ctx.ChargeLevel * 100);
            // int manaCost = Mathf.RoundToInt(10 + ctx.ChargeLevel * 20);

            //if (HasEnoughMana(manaCost))
            //{
            //    ConsumeMana(manaCost);

            //    // 獲取瞄準方向
            //    Vector3 aimDirection = GetAimDirection();

            //    // 發布施法事件
            //    PublishEvent(new SpellCastEvent
            //    {
            //        SpellPower = spellPower,
            //        Direction = aimDirection,
            //        ChargeLevel = context.ChargeLevel,
            //        CastPosition = context.Transform.position + Vector3.up * 1.5f
            //    });

            //    Debug.Log($"施放魔法！蓄力: {context.ChargeLevel:P0}, 威力: {spellPower}, 魔力消耗: {manaCost}");
            //}
            //else
            //{
            //    Debug.Log("魔力不足！");
            //    PlaySound("ManaNone");
            //}

            //// 重置蓄力等級
            //context.ChargeLevel = 0f;
        }

        public override bool CanTransitionTo(Type stateType)
        {
            // 施法期間只能被打斷為受擊/死亡
            if (stateType == typeof(HitState) || stateType == typeof(DyingState))
                return true;

            return stateTimer <= 0;
        }
    }
}
