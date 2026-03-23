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
    public class MagicStatusUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image MageCDBuffer;
        [Inject]private readonly IEventBus _eventBus;
        private CancellationTokenSource _cts;
        private StatusBarController _mgCDBufCtrl;

        private void OnEnable()
        {

            _cts = new CancellationTokenSource();
            _mgCDBufCtrl = new StatusBarController(MageCDBuffer);
            // 訂閱事件
            _eventBus.Subscribe<PlayerWeaponFireEvent>(OnMagicFire);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _mgCDBufCtrl.Dispose();
            // 取消訂閱
            _eventBus.Unsubscribe<PlayerWeaponFireEvent>(OnMagicFire);
        }
        private CancellationTokenSource MagicCoolTimeTokenSoure;

        private void OnMagicFire(PlayerWeaponFireEvent evt)
        {
            var tc = MageCDBuffer.color;
            tc.a = 0.5f;
            MageCDBuffer.color = tc;
            tc.a = 1f;
            _mgCDBufCtrl.FillAmount = 0f;            

            _mgCDBufCtrl.FillType.LerpUpdate(1f, evt.bulletConfig.CoolTime);
            _mgCDBufCtrl.ColorType.LerpUpdate(tc, evt.bulletConfig.CoolTime);

        }
        private void UpdateBarBufferWithDelay(Image bufferBar, float duration, CancellationTokenSource bufferTokenSoure)
        {
            bufferBar.fillAmount = 0f;
            SmoothBufferUpdate(bufferBar, 1f, duration, MagicCoolTimeTokenSoure).Forget();
        }   
        
        private async UniTaskVoid SmoothBufferUpdate(Image bufferBar, float targetRatio, float duration, CancellationTokenSource bufferTokenSoure)
        {
            // 這裡維持你的邏輯：殺掉正在跑的舊動畫，但不影響 PRXY 的 Delay
            bufferTokenSoure?.Cancel();
            bufferTokenSoure?.Dispose();
            bufferTokenSoure = new CancellationTokenSource();
            var token = bufferTokenSoure.Token;
            float startFill = bufferBar.fillAmount;
            float elapsed = 0f;
            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    //1f - Mathf.Pow(1f - (elapsed / duration), 2);
                    float t = elapsed / duration;

                    bufferBar.fillAmount = Mathf.Lerp(startFill, targetRatio, t);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
                bufferBar.fillAmount = targetRatio;
            }
            catch (System.OperationCanceledException) { }
        }
 
        
    }
}
