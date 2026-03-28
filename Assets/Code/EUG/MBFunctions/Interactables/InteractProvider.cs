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
    public abstract class InteractProvider : MonoBehaviour, IInteractable
    {
        private IEventBus _eventBus;
        private bool _canInteract;
        private Collider _collider;
        protected IEventBus EventBus { get => _eventBus; }
        protected Collider Collider { get => _collider; }

        protected LayerMask layerMask;
        
        public abstract string InteractableName { get; }
        public abstract float InteractionDuration { get; }
        public abstract bool ReInteractable { get; }
        protected abstract void OnEnter();
        protected abstract void OnExit();
        private void Awake()
        {
            _eventBus = EventBusProvider.GetEventBus();
            layerMask = LayerMask.GetMask("Player");
            _canInteract = true;
        }
        protected virtual void InnerOnInteract()
        {
            OnInteract();
            if (!ReInteractable)
            {
                _canInteract = false;
                OnExit();
            }
        }
        public abstract void OnInteract();
        private void OnTriggerEnter(Collider other)
        {
            if ((layerMask | 1 << other.gameObject.layer) != 0)
            {
                _collider = other;
                // 發布事件
                if (_canInteract)
                    OnEnter();
                else
                    OnExit();
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if ((layerMask | 1 << other.gameObject.layer) != 0)
            {
                // 發布事件
                OnExit();
            }
        }
    }
}
