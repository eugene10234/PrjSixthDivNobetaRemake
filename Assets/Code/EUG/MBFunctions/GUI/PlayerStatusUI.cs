using Cysharp.Threading.Tasks;
using Nobeta.EUG.Interfaces;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
// using TMPro; // 如果你有用到文字

namespace Nobeta.EUG.MBFunctions.PlayerUI
{
    public class PlayerStatusUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image healthBar;
        [SerializeField] private Image healthBuffer;
        [SerializeField] private Image manaBar;
        [SerializeField] private Image staminaBar;
        [Inject]private readonly IEventBus _eventBus;
        private CancellationTokenSource _cts;
        [Header("Settings")]
        [SerializeField] private float duration = 2.0f; // 動畫時長
        private float Testduration = 1.0f; 
        private void OnEnable()
        {

            _cts = new CancellationTokenSource();
            // 訂閱事件
            _eventBus.Subscribe<PlayerStatusUpdateEvent>(OnPlayerStatusChanged);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            // 取消訂閱，防止記憶體洩漏
            _eventBus.Unsubscribe<PlayerStatusUpdateEvent>(OnPlayerStatusChanged);
        }
        private CancellationTokenSource healthTokenSoure;
        private CancellationTokenSource manaTokenSoure;
        private CancellationTokenSource staminaTokenSoure;
        private void OnPlayerStatusChanged(PlayerStatusUpdateEvent evt)
        {
            // 更新血條 (Fill Amount 範圍是 0 ~ 1)
            //if (healthBar != null)
            //    healthBar.fillAmount = evt.CurrentHealth / evt.MaxHealth;
            //UpdateBarSmoothly(healthBar, evt.CurrentHealth / evt.MaxHealth).Forget();
            UpdateBarBufferWithDelay(healthBar,healthBuffer, evt.CurrentHealth / evt.MaxHealth, healthTokenSoure);
            // 更新魔力條
            if (manaBar != null)
                manaBar.fillAmount = evt.CurrentMana / evt.MaxMana;

            // 更新耐力條
            if (staminaBar != null)
                staminaBar.fillAmount = evt.CurrentStamina / evt.MaxStamina;

            // 提示：如果要讓動畫更平滑，建議配合 DOTween 使用
            // healthBar.DOFillAmount(evt.CurrentHealth / evt.MaxHealth, 0.2f);
        }
        private void UpdateBarBufferWithDelay(Image Bar, Image Buffer, float targetRatio, CancellationTokenSource bufferTokenSoure)
        {
            if (!Bar || !Buffer ) return;
            if (targetRatio > Bar.fillAmount)
            {
                // 補血：兩者同步增加
                Bar.fillAmount = targetRatio;
                Buffer.fillAmount = targetRatio;
                return;
            }
            healthBar.fillAmount = targetRatio;
            DelayTriggerBufferSequence(Buffer, targetRatio, bufferTokenSoure).Forget();
        }
        // 你的 TriggerBufferSequence (負責計時與發號施令)
        private async UniTaskVoid DelayTriggerBufferSequence(Image BufferBar, float targetRatio, CancellationTokenSource bufferTokenSoure)
        {
            var token = _cts.Token;
            try
            {
                //等待緩衝時間
                await UniTask.Delay(500, cancellationToken: token);
                // 延遲結束，發起動畫
                SmoothBufferUpdate(BufferBar, targetRatio, bufferTokenSoure).Forget();
            }
            catch (System.OperationCanceledException) { /* 正常取消 */ }
        }
        
        private async UniTask SmoothBufferUpdate(Image bufferBar, float targetRatio, CancellationTokenSource bufferTokenSoure)
        {
            // 這裡維持你的邏輯：殺掉正在跑的舊動畫，但不影響 TriggerBufferSequence 的 Delay
            bufferTokenSoure?.Cancel();
            bufferTokenSoure?.Dispose();
            bufferTokenSoure = new CancellationTokenSource();
            var token = bufferTokenSoure.Token;
            float startFill = bufferBar.fillAmount;
            float elapsed = 0f;
            float duration = 0.5f;
            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = 1f - Mathf.Pow(1f - (elapsed / duration), 2);

                    bufferBar.fillAmount = Mathf.Lerp(startFill, targetRatio, t);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
                bufferBar.fillAmount = targetRatio;
            }
            catch (System.OperationCanceledException) { }
        }
       
    }
}
