using System;
using System.Threading;
using Assets.Code.EUG.MBFunctions.GUI;
using Cysharp.Threading.Tasks;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.MBFunctions.Interacts;
using Nobeta.EUG.Services;
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
        private StatusBarController _iconEreaCtrl;
        private StatusBarController _iconColorCtrl;
        private StatusBarController _iconBackCtrl;
        private float VisDuration = 0.2f;
        private Chest _currentOBJ;

        private void OnEnable()
        {
            _playerInputService.OnInteract += OnInteractPress;
            _eventBus.Subscribe<InteractEreaEvent>(OnEreaTrigger);
        }

        private void OnDisable()
        {
            _playerInputService.OnInteract -= OnInteractPress;
            _eventBus.Unsubscribe<InteractEreaEvent>(OnEreaTrigger);
        }

        private void Awake()
        {
            //_eventBus = EventBusProvider.GetEventBus();
        }
        private void OnInteractPress()
        {
            _currentOBJ.OnInteract();
        }
        private void OnEreaTrigger(InteractEreaEvent evt)
        {
            _Text.text = evt.Message;
            TextBar.SetActive(evt.Enter);
            _currentOBJ = evt.chest;
            //if (evt.Enter)
            //{

            //}
        }

        private void SubscribeEvents()
        {
            //_eventBus.Subscribe<PlayerAimingStartEvent>(OnPlayerAimStart);
            //_eventBus.Subscribe<PlayerAimingEndEvent>(OnPlayerAimEnd);
            _eventBus.Subscribe<PlayerLiveStateEvent>(OnPlayerLiveStateChange);
        }

        private void UnsubscribeEvents()
        {
            //_eventBus.Unsubscribe<PlayerAimingStartEvent>(OnPlayerAimStart);
            //_eventBus.Unsubscribe<PlayerAimingEndEvent>(OnPlayerAimEnd);
            _eventBus.Unsubscribe<PlayerLiveStateEvent>(OnPlayerLiveStateChange);
        }
        private void OnPlayerLiveStateChange(PlayerLiveStateEvent evt)
        {
            // isPlayerLive = evt.IsLive;
        }
    }
}
