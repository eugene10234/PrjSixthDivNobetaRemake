using Assets.Code.EUG.MBFunctions.GUI;
using Cysharp.Threading.Tasks;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using System.Threading;
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
        [SerializeField] private Image NormalIconColor;
        [SerializeField] private Image NormalIconBack;
        [Inject]private readonly IEventBus _eventBus;
        private CancellationTokenSource _cts;
        private StatusBarController _iconColorCtrl;
        private StatusBarController _iconBackCtrl;

        private void OnEnable()
        {

            _cts = new CancellationTokenSource();
            _iconColorCtrl = new StatusBarController(NormalIconColor);
            _iconBackCtrl = new StatusBarController(NormalIconBack);
            // 訂閱事件
            //_eventBus.Subscribe<WeaponFireEvent>(OnMagicFire);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _iconColorCtrl.Dispose();
            _iconBackCtrl.Dispose();
            // 取消訂閱
           // _eventBus.Unsubscribe<WeaponFireEvent>(OnMagicFire);
        }
        private CancellationTokenSource MagicCoolTimeTokenSoure;

        private void OnIconAimStart()
        {


        }
        private void OnIconAimEnd()
        {


        }
        private void OnIconFireStart()
        {


        }
        private void OnIconFireEnd()
        {


        }



    }
}
