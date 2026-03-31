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
//Collider Needed!!!!!!!!!!!!!!!!!!

namespace Nobeta.EUG.MBFunctions.Interacts
{
    public class Chest : InteractProvider
    {
        [SerializeField] private Animator _animator;
        public override string InteractableName => "Chest";

        public override float InteractionDuration => 0.1f;

        public override bool ReInteractable => false;

        private void OnEnable()
        {
            CanAttack = true;
        }
        public override void OnInteract()
        {
            if (IsWeaponInteracted)
                _animator.Play("OpenStart",0, 0.5f);
            else
                _animator.Play("OpenStart");
        }

        protected override void OnEnter()
        {
            EventBus.Publish(new InteractEreaEnterEvent
            {
                InteractObj = this,
                Message = $"[E]開啟寶箱"
            });
        }

        protected override void OnExit()
        {
            EventBus.Publish(new InteractEreaExitEvent
            {
                InteractObj = this,
                Message = ""
            });
        }
    }
}
