
//using System;
//using Nobeta.EUG.AANEW.MBbulletsystems;
//using Nobeta.EUG.BulletSystemV1.OOP.UnityBridge;
//using Nobeta.EUG.Events;
//using Nobeta.EUG.FSM.Core;
//using Nobeta.EUG.FSM.Services;
//using Nobeta.EUG.Interfaces;
//using UnityEngine;
//using VContainer;
//// ========================================================================
//// ChargingState - 蓄力狀態
//// ========================================================================
//namespace Nobeta.EUG.FSM.States
//{

//    /// <summary>
//    /// 蓄力狀態
//    /// 玩家按住攻擊鍵蓄力，增加魔法威力
//    /// </summary>
//    public class ChargingState : CombatState//, IInputHandler
//    {
//        public ChargingState(StateContext context, StateMachine stateMachine)
//            : base(context, stateMachine)
//        {
//        }

//        public override void Enter()
//        {
//            base.Enter();

//            ctx.ChargeStartTime = Time.time;
//            //ctx.ChargeLevel = 0f;

//            PlayAnimation("Charge");
//            PlaySound("ChargeStart");

//            // 停止移動
//            ctx.MovementService.ResetVelocity();
//        }

//        public void HandleInput()
//        {
//            //// 釋放施法
//            //if (Input.GetMouseButtonUp(0))
//            //{
//            //    stateMachine.ChangeState<CastingState>();
//            //    return;
//            //}

//            //// 取消蓄力（放開右鍵）
//            //if (Input.GetMouseButtonUp(1))
//            //{
//            //    ctx.ChargeLevel = 0f;
//            //    stateMachine.ChangeState<IdleState>();
//            //    return;
//            //}
//        }

//        public override void Update()
//        {
//            base.Update();

//            //// 更新蓄力等級
//            //float chargeTime = Time.time - ctx.ChargeStartTime;
//            //ctx.ChargeLevel = Mathf.Clamp01(chargeTime / ctx.Config.maxChargeTime);

//            //// 更新動畫參數
//            //ctx.AnimationService.SetFloat("ChargeLevel", ctx.ChargeLevel);

//            //// 最大蓄力時自動釋放
//            //if (ctx.ChargeLevel >= 1f)
//            //{
//            //    stateMachine.ChangeState<CastingState>();
//            //}

//            //// 持續面向瞄準方向
//            //Vector3 aimDirection = GetAimDirection();
//            //context.MovementService.RotateTowards(aimDirection, context.Config.rotationSpeed);
//        }

//        // 輸入事件處理
//        public override void OnDodgeInput()
//        {
//            if (ctx.CanDodge)
//            {
//                // 取消蓄力並閃避
//                //ctx.ChargeLevel = 0f;
//                stateMachine.ChangeState<DodgingState>();
//            }
//        }

//        public override bool CanTransitionTo(System.Type stateType)
//        {
//            // 蓄力期間可以被打斷
//            if (stateType == typeof(HitState) || stateType == typeof(DyingState))
//                return true;

//            if (stateType == typeof(DodgingState))
//                return true;

//            return base.CanTransitionTo(stateType);
//        }
//    }
//}
