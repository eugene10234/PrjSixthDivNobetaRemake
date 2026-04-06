using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.Interfaces;
using Nobeta.Sam.Events;
using System;
using System.Net;
using Unity.Mathematics;

using UnityEngine;
using VContainer.Unity;
// ========================================================================
// InteractingState - 通用互動狀態
// ========================================================================
namespace Nobeta.EUG.FSM.States
{

    /// <summary>
    /// 通用互動狀態
    /// 用於與環境物體互動（開門、拉桿、拾取等）
    /// </summary>
    public class InteractingState : InteractionState
    {
        private IInteractable currentInteractable;

        public InteractingState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
            interactionDuration = 1f; // 默認互動時間
        }

        public override void Enter()
        {
            base.Enter();

            PlayAnimation("Interact");

            // 執行互動
            Debug.Log("開始互動...");
        }

        protected override void OnInteractionComplete()
        {
            Debug.Log("互動完成！");

            // 發布互動完成事件
            PublishEvent(new DialogNextEvent
            {
                //InteractableType = "Generic",
                //Position = ctx.Transform.position
            });
            
            base.OnInteractionComplete();
        }

        /// <summary>
        /// 設置互動對象和時間
        /// </summary>
        public void SetInteraction(IInteractable interactable, float duration)
        {
            currentInteractable = interactable;
            interactionDuration = duration;
        }
    }
}
