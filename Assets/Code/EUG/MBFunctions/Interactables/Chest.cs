using System.Threading;
using Assets.Code.EUG.MBFunctions.GUI;
using Cysharp.Threading.Tasks;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.MBFunctions.PlayerUI;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
// using TMPro; // 如果你有用到文字

namespace Nobeta.EUG.MBFunctions.Interacts
{
    public class Chest : InteractProvider
    {
        [SerializeField] private Animator _animator;
        public override string InteractableName => "寶箱";

        public override float InteractionDuration => 0.1f;

        public override bool ReInteractable => false;

        public override void OnInteract()
        {
            _animator.Play("OpenStart");
        }

        protected override void OnEnter()
        {
            EventBus.Publish(new InteractEreaEnterEvent
            {
                Message = $"[E]開啟{InteractableName}",
                InteractObj = this
            });
        }

        protected override void OnExit()
        {
            EventBus.Publish(new InteractEreaExitEvent
            {
                Message = "",
                InteractObj = this
            });
        }
    }

}
