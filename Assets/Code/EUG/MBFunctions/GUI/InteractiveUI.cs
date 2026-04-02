using Assets.Code.EUG.MBFunctions.GUI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.MBFunctions.Interacts;
using Nobeta.EUG.Services;
using System;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
// using TMPro; // 如果你有用到文字

namespace Nobeta.EUG.MBFunctions.PlayerUI
{
    public class InteractiveUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject TextBar;
        [SerializeField] private Text _Text;
        [Inject] private readonly IEventBus _eventBus;
        [Inject] private readonly IPlayerInputService _playerInputService;
        private CancellationTokenSource _cts;
        private IInteractable _currentOBJ;

        private void OnEnable()
        {
            _playerInputService.OnInteract += OnInteractPress;
            _eventBus.Subscribe<InteractEreaEnterEvent>(OnEreaEnter);
            _eventBus.Subscribe<InteractEreaExitEvent>(OnEreaExit);
        }

        private void OnDisable()
        {
            _playerInputService.OnInteract -= OnInteractPress;
            _eventBus.Unsubscribe<InteractEreaEnterEvent>(OnEreaEnter);
            _eventBus.Unsubscribe<InteractEreaExitEvent>(OnEreaExit);
        }

        private void Awake()
        {
            //_eventBus = EventBusProvider.GetEventBus();
        }
        private void OnInteractPress()
        {
            _currentOBJ.OnInteract();
        }
        private void OnEreaEnter(InteractEreaEnterEvent evt)
        {
            _Text.text = evt.Message;
            TextBar.SetActive(true);
            _currentOBJ = evt.InteractObj;
            //if (evt.Enter)
            //{

            //}
        }
        private void OnEreaExit(InteractEreaExitEvent evt)
        {
            EreaExit(evt);
        }

        private void EreaExit(InteractEreaExitEvent evt)
        {
            //_Text.text = evt.Message;
            //_currentOBJ = evt.InteractObj;
            _Text.text = "";
            TextBar.SetActive(false);
            _currentOBJ = null;
            //SequenceFade(evt);
        }
        //Sequence mySequence = DOTween.Sequence();
        public void SequenceFade(InteractEreaExitEvent evt)
        {
            Sequence mySequence = DOTween.Sequence();

            mySequence.AppendInterval(3f)
                      .OnComplete(() =>
                      {
                          _Text.text = "";
                          TextBar.SetActive(false);
                          _currentOBJ = null;
                      });
        }
        private void SubscribeEvents()
        {

            _eventBus.Subscribe<PlayerLiveStateEvent>(OnPlayerLiveStateChange);
        }

        private void UnsubscribeEvents()
        {

            _eventBus.Unsubscribe<PlayerLiveStateEvent>(OnPlayerLiveStateChange);
        }
        private void OnPlayerLiveStateChange(PlayerLiveStateEvent evt)
        {
            // isPlayerLive = evt.IsLive;
        }
    }
}
