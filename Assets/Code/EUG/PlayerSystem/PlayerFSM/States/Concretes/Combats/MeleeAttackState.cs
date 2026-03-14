using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Services;
using Nobeta.EUG.Interfaces;
using System;
using UnityEngine;
// ========================================================================
// MeleeAttackState - 近戰攻擊狀態
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    /// <summary>
    /// 近戰攻擊狀態
    /// 使用近戰武器攻擊
    /// </summary>
    public class MeleeAttackState : CombatState
    {
        private int comboCounter = 0;
        private float lastAttackTime = 0f;
        private const float COMBO_WINDOW = 1f;

        public MeleeAttackState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            ctx.MovementService.RootMotionStart();
            stateTimer = ctx.Config.meleeAttackDuration;

            // 檢查連擊
            if (Time.time - lastAttackTime < COMBO_WINDOW)
            {
                ctx.ComboCounter = (ctx.ComboCounter + 1) % 3; // 最多3連擊
            }
            else
            {
                ctx.ComboCounter = 0;
            }

            lastAttackTime = Time.time;
            ctx.LastAttackTime = Time.time;

            // 執行攻擊
            PerformMeleeAttack();

            PlayAnimation($"MeleeAttack_{ctx.ComboCounter + 1}");
            PlaySound("MeleeAttack");

            // 停止移動
            ctx.MovementService.ResetVelocity();
        }
        public override void Exit()
        {
            base.Exit();
            ctx.MovementService.RootMotionStart();
            // 攻擊結束後重置連擊計數（如果沒有在連擊窗口內）
            if (Time.time - lastAttackTime >= COMBO_WINDOW)
            {
                ctx.ComboCounter = 0;
            }
        }
        protected override void OnTimerExpired()
        {

            // 攻擊完成
            if (ctx.InputService.MoveInput.magnitude > 0.1f)
            {
                stateMachine.ChangeState<WalkingState>();
            }
            else
            {
                stateMachine.ChangeState<IdleState>();
            }
        }

        private void PerformMeleeAttack()
        {
            // 計算傷害（連擊增加傷害）
            int baseDamage = 10;
            int damage = baseDamage + (ctx.ComboCounter * 5);

            // 發布攻擊事件
            //PublishEvent(new MeleeAttackEvent
            //{
            //    Damage = damage,
            //    ComboCount = ctx.ComboCounter + 1,
            //    AttackPosition = ctx.Transform.position,
            //    AttackDirection = ctx.Transform.forward
            //});

            Debug.Log($"執行近戰攻擊！連擊數: {ctx.ComboCounter + 1}, 傷害: {damage}");
        }

        public override void OnAttackInput()
        {
            // 在攻擊動畫後半段可以連擊
            if (stateTimer < ctx.Config.meleeAttackDuration * 0.5f)
            {
                stateMachine.ChangeState<MeleeAttackState>();
            }
        }

        public override bool CanTransitionTo(System.Type stateType)
        {
            // 攻擊期間可以連擊
            if (stateType == typeof(MeleeAttackState))
                return stateTimer < ctx.Config.meleeAttackDuration * 0.5f;

            // 可以被打斷
            if (stateType == typeof(HitState) || stateType == typeof(DyingState))
                return true;

            return stateTimer <= 0;
        }
    }
}
