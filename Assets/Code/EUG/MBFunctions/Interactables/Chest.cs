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
    public class Chest : MonoBehaviour
    {
        private IEventBus _eventBus;
        [SerializeField] private Animator _animator;
        private bool _canInteact;

        private void Awake()
        {
            _eventBus = EventBusProvider.GetEventBus();
            _canInteact = true;
        }
        public void OnInteract()
        {
            _animator.Play("OpenStart");
            _canInteact = false;
            OnExit();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                // 發布事件
                if (_canInteact)
                    OnEnter();
                else
                    OnExit();
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                // 發布事件
                OnExit();
            }
        }
        private void OnEnter()
        {
            _eventBus.Publish(new InteractEreaEnterEvent
            {
                Enter = true,
                Message = "[E]開啟OAQ",
                chest = this

            });
        }
        private void OnExit()
        {
            _eventBus.Publish(new InteractEreaExitEvent
            {
                Enter = false,
                Message = ""
            });
        }
    }
}
