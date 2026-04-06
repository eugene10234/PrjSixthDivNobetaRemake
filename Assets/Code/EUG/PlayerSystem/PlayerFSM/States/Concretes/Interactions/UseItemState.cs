//using Nobeta.EUG.FSM.Core;
//using System;
//using System.Net;
//using Unity.Mathematics;

//using UnityEngine;
//using VContainer.Unity;
//// ========================================================================
//// UseItemState - 使用道具狀態
//// ========================================================================
//namespace Nobeta.EUG.FSM.States
//{
//    /// <summary>
//    /// 使用道具狀態
//    /// 玩家使用消耗品或工具
//    /// 
//    /// 這個狀態可以算是 InteractionState 的一種！
//    /// 因為使用道具是一種「與道具互動」的行為
//    /// </summary>
//    public class UseItemState : InteractionState
//    {
//        private ItemData currentItem;

//        public UseItemState(StateContext context, StateMachine stateMachine)
//            : base(context, stateMachine)
//        {
//            interactionDuration = 0.8f; // 使用道具的動畫時長
//        }

//        public override void Enter()
//        {
//            base.Enter();

//            // 播放使用道具動畫
//            PlayAnimation("UseItem");
//            PlaySound("ItemUse");

//            Debug.Log($"使用道具: {currentItem?.ItemName ?? "未知"}");
//        }

//        public override void Update()
//        {
//            base.Update();

//            // 在動畫中途應用道具效果
//            if (interactionTimer <= interactionDuration * 0.5f && currentItem != null)
//            {
//                ApplyItemEffect();
//                currentItem = null; // 防止重複應用
//            }
//        }

//        protected override void OnInteractionComplete()
//        {
//            Debug.Log("道具使用完成！");

//            base.OnInteractionComplete();
//        }

//        /// <summary>
//        /// 設置要使用的道具
//        /// </summary>
//        public void SetItem(ItemData item)
//        {
//            currentItem = item;

//            // 根據道具類型調整動畫時長
//            if (item != null)
//            {
//                interactionDuration = item.UseDuration;
//            }
//        }

//        /// <summary>
//        /// 應用道具效果
//        /// </summary>
//        private void ApplyItemEffect()
//        {
//            if (currentItem == null) return;

//            switch (currentItem.ItemType)
//            {
//                case ItemType.HealthPotion:
//                    ApplyHealthPotion(currentItem.EffectValue);
//                    break;

//                case ItemType.ManaPotion:
//                    ApplyManaPotion(currentItem.EffectValue);
//                    break;

//                case ItemType.Buff:
//                    ApplyBuff(currentItem);
//                    break;

//                case ItemType.Tool:
//                    UseTool(currentItem);
//                    break;

//                default:
//                    Debug.LogWarning($"未處理的道具類型: {currentItem.ItemType}");
//                    break;
//            }

//            // 發布道具使用事件
//            PublishEvent(new ItemUsedEvent
//            {
//                ItemName = currentItem.ItemName,
//                ItemType = currentItem.ItemType,
//                EffectValue = currentItem.EffectValue
//            });
//        }

//        private void ApplyHealthPotion(int healAmount)
//        {
//            int oldHealth = ctx.CurrentHealth;
//            ctx.CurrentHealth = Mathf.Min(
//                ctx.CurrentHealth + healAmount,
//                ctx.MaxHealth
//            );

//            int actualHeal = ctx.CurrentHealth - oldHealth;

//            Debug.Log($"恢復生命值: {actualHeal}");
//            PlaySound("Heal");

//            // 發布治療事件
//            PublishEvent(new PlayerHealedEvent
//            {
//                Amount = actualHeal,
//                CurrentHealth = ctx.CurrentHealth
//            });
//        }

//        private void ApplyManaPotion(int manaAmount)
//        {
//            int oldMana = ctx.CurrentMana;
//            ctx.CurrentMana = Mathf.Min(
//                ctx.CurrentMana + manaAmount,
//                ctx.MaxMana
//            );

//            int actualRestore = ctx.CurrentMana - oldMana;

//            Debug.Log($"恢復魔力: {actualRestore}");
//            PlaySound("ManaRestore");

//            // 發布魔力恢復事件
//            PublishEvent(new ManaRestoredEvent
//            {
//                Amount = actualRestore,
//                CurrentMana = ctx.CurrentMana
//            });
//        }

//        private void ApplyBuff(ItemData item)
//        {
//            Debug.Log($"應用增益效果: {item.ItemName}");

//            // 發布增益事件
//            PublishEvent(new BuffAppliedEvent
//            {
//                BuffName = item.ItemName,
//                Duration = item.BuffDuration,
//                EffectValue = item.EffectValue
//            });
//        }

//        private void UseTool(ItemData item)
//        {
//            Debug.Log($"使用工具: {item.ItemName}");

//            // 發布工具使用事件
//            PublishEvent(new ToolUsedEvent
//            {
//                ToolName = item.ItemName,
//                Position = ctx.Transform.position
//            });
//        }

//        public override bool CanTransitionTo(System.Type stateType)
//        {
//            // 使用道具可以被打斷（比較快）
//            if (stateType == typeof(HitState) || stateType == typeof(DyingState))
//                return true;

//            // 閃避可以取消使用道具
//            if (stateType == typeof(DodgingState))
//                return true;

//            return interactionTimer <= 0;
//        }
//    }
//}

