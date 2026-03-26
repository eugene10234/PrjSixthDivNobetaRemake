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
        private void Awake()
        {
            _eventBus = EventBusProvider.GetEventBus();
            //_animator.Play("OpenStart");
        }
        public void OnInteract()
        {
            _animator.Play("OpenStart");
        }
        // Update is called once per frame
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                // 發布事件
                //_eventBus.Publish(new PlayerDamagedEvent
                //{
                //    Damage = 99999,
                //    hitPosition = transform.position
                //});
                _eventBus.Publish(new InteractEreaEvent
                {
                    Enter = true,
                    Message = "[E]開啟OAQ",
                    chest = this

                });
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                // 發布事件
                _eventBus.Publish(new InteractEreaEvent
                {
                    Enter = false,
                    Message = ""
                });
            }
        }

    }
}
