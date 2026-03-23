using System;
using System.Threading;
using Assets.Code.EUG.MBFunctions.GUI;
using Cysharp.Threading.Tasks;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
// using TMPro; // 如果你有用到文字

namespace Nobeta.EUG.MBFunctions.PlayerUI
{
    public class AimIconUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform NormalIconErea;
        [SerializeField] private Image NormalIconColor;
        [SerializeField] private Image NormalIconBack;
        [Inject] private readonly IEventBus _eventBus;
        [Inject] private readonly IPlayerInputService _playerInputService;
        private CancellationTokenSource _cts;
        private StatusBarController _iconEreaCtrl;
        private StatusBarController _iconColorCtrl;
        private StatusBarController _iconBackCtrl;
        private float VisDuration = 0.35f;
        private void OnEnable()
        {
            _cts = new CancellationTokenSource();
            _iconEreaCtrl = new StatusBarController(NormalIconErea);
            _iconColorCtrl = new StatusBarController(NormalIconColor);
            _iconBackCtrl = new StatusBarController(NormalIconBack);
            // 訂閱事件
            AssignInputEvents();
            SubscribeEvents();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _iconEreaCtrl?.Dispose();
            _iconColorCtrl?.Dispose();
            _iconBackCtrl?.Dispose();
            // 取消訂閱
            UnassignInputEvents();
            UnsubscribeEvents();
        }
        private bool IsAimPressed = false;
        private bool IsFirePressed = false;
        private void OnProgress()
        {
            if (!IsAimPressed && !IsFirePressed) { OnDisVis(); return; }
            if (IsAimPressed) { OnAimSet(); return; }
            OnFireSet();
            return;
        }
        private void OnDisVis()
        {
            Color tc = NormalIconColor.color; //_iconColorCtrl
            tc.a = 0f;
            Color tb = NormalIconBack.color;
            tb.a = 0f;
            Vector3 ts = NormalIconErea.localScale;
            ts = Vector3.one ;

            _iconEreaCtrl.RectType.LerpUpdate(ts, VisDuration);
            _iconColorCtrl.ColorType.LerpUpdate(tc, VisDuration);
            _iconBackCtrl.ColorType.LerpUpdate(tb, VisDuration);
            return;
        }
        private void OnAimSet()
        {
            //Debug.Log($"{DateTime.Now}OnAimSet!!!!");
            Color tc = NormalIconColor.color; 
            tc.a = 1f;
            Color tb = NormalIconBack.color;
            tb.a = 0.2f;
            Vector3 ts = NormalIconErea.localScale;
            ts = Vector3.one * 0.4f;

            _iconEreaCtrl.RectType.LerpUpdate(ts, VisDuration);
            _iconColorCtrl.ColorType.LerpUpdate(tc, VisDuration);
            _iconBackCtrl.ColorType.LerpUpdate(tb, VisDuration);
            return;
        }
        private void OnFireSet()
        {
            Color tc = NormalIconColor.color; 
            tc.a = 1f;
            Color tb = NormalIconBack.color;
            tb.a = 0.2f;
            Vector3 ts = NormalIconErea.localScale;
            ts = Vector3.one;

            _iconEreaCtrl.RectType.LerpUpdate(ts, VisDuration);
            _iconColorCtrl.ColorType.LerpUpdate(tc, VisDuration);
            _iconBackCtrl.ColorType.LerpUpdate(tb, VisDuration);
            return;
        }
        private void OnIconAimStart() { IsAimPressed = true; OnProgress(); }
        private void OnIconAimEnd() { IsAimPressed = false; OnProgress(); }
        private void OnIconFireStart() { IsFirePressed = true; OnProgress(); }
        private void OnIconFireEnd() { IsFirePressed = false; OnProgress(); }
        private void OnPlayerAimStart(PlayerAimingStartEvent evt) { OnIconAimStart(); }
        private void OnPlayerAimEnd(PlayerAimingEndEvent evt) { OnIconAimEnd(); }
        private void AssignInputEvents()
        {
            _playerInputService.OnAimStart += OnIconAimStart;
            _playerInputService.OnFireStart += OnIconFireStart;
            _playerInputService.OnAimEnd += OnIconAimEnd;
            _playerInputService.OnFireEnd += OnIconFireEnd;
        }
        private void UnassignInputEvents()
        {
            _playerInputService.OnAimStart -= OnIconAimStart;
            _playerInputService.OnFireStart -= OnIconFireStart;
            _playerInputService.OnAimEnd -= OnIconAimEnd;
            _playerInputService.OnFireEnd -= OnIconFireEnd;
        }

        private void SubscribeEvents()
        {
            //_eventBus.Subscribe<PlayerAimingStartEvent>(OnPlayerAimStart);
            //_eventBus.Subscribe<PlayerAimingEndEvent>(OnPlayerAimEnd);
        }

        private void UnsubscribeEvents()
        {
            //_eventBus.Unsubscribe<PlayerAimingStartEvent>(OnPlayerAimStart);
            //_eventBus.Unsubscribe<PlayerAimingEndEvent>(OnPlayerAimEnd);
        }
    }
}
