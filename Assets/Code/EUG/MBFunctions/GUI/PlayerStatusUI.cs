using Assets.Code.EUG.MBFunctions.GUI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Nobeta.EUG.Interfaces;
using System;
using System.Threading;
using Unity.VisualScripting;
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
        [SerializeField] private Image healthWarning;
        [SerializeField] private Image manaBar;
        [SerializeField] private Image manaBuffer;
        [SerializeField] private Image staminaBar;
        [Inject]private readonly IEventBus _eventBus;
        private CancellationTokenSource _cts;
        [Header("Settings")]
        [SerializeField] private float duration = 2.0f; // 動畫時長
        private StatusBarController _hpCtrl;
        private StatusBarController _hpBufCtrl;
        private StatusBarController _manaCtrl;
        private StatusBarController _manaBufCtrl;
        private StatusBarController _staminaCtrl;
        private Color healthWarningPrototype;
        private void OnEnable()
        {
            healthWarningPrototype = healthWarning.color;
            _cts = new CancellationTokenSource();
            _hpCtrl = new StatusBarController(healthBar);
            _hpBufCtrl = new StatusBarController(healthBuffer);
            _manaCtrl = new StatusBarController(manaBar);
            _manaBufCtrl = new StatusBarController(manaBuffer);
            _staminaCtrl = new StatusBarController(staminaBar);
            // 訂閱事件
            _eventBus.Subscribe<PlayerStatusUpdateEvent>(OnPlayerStatusChanged);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _hpCtrl.Dispose();
            _hpBufCtrl.Dispose();
            _manaCtrl.Dispose();
            _manaBufCtrl.Dispose();
            _staminaCtrl.Dispose();
            // 取消訂閱，防止記憶體洩漏
            _eventBus.Unsubscribe<PlayerStatusUpdateEvent>(OnPlayerStatusChanged);
        }

        //private CancellationTokenSource staminaTokenSoure;
        private void OnPlayerStatusChanged(PlayerStatusUpdateEvent evt)
        {
            UpdateBarBufferWithDelay(_hpCtrl, _hpBufCtrl, evt.CurrentHealth / evt.MaxHealth);
            // 更新魔力條

            UpdateBarBufferWithDelay(_manaCtrl, _manaBufCtrl, evt.CurrentMana / evt.MaxMana);
            // 更新耐力條
            //if (staminaBar != null)
            //    staminaBar.fillAmount = evt.CurrentStamina / evt.MaxStamina;
            _staminaCtrl.FillType.LerpUpdate(evt.CurrentStamina / evt.MaxStamina,0.15f);
            // 提示：如果要讓動畫更平滑，建議配合 DOTween 使用
            // healthBar.DOFillAmount(evt.CurrentHealth / evt.MaxHealth, 0.2f);
            hpWarning(evt.CurrentHealth / evt.MaxHealth);
        }

        private void hpWarning(float hpratio)
        {
            
            // 在血量變更的邏輯裡
            if (hpratio <= 0.35f)
            {
                // 檢查是否已經在閃爍，避免重複播放
                if (!DOTween.IsTweening("HPFlash"))
                {
                    healthWarningPrototype.a = 0.75f;
                    healthWarning.DOColor(healthWarningPrototype, 0.25f) // 速度極快
                        .SetEase(Ease.Linear)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetId("HPFlash");
                }
            }
            else
            {
                healthWarningPrototype.a = 0.2f;
                DOTween.Kill("HPFlash");
                healthWarning.color = healthWarningPrototype;
            }
        }

        private void UpdateBarBufferWithDelay(StatusBarController Bar, StatusBarController Buffer, float targetRatio)
        {      
            if (targetRatio > Bar.FillAmount)
            {
                Bar.FillAmount = targetRatio;
                Buffer.DelayUpdateTrigger(targetRatio).Forget();
                return;
            }
            Bar.FillAmount = targetRatio;
            Buffer.DelayUpdateTrigger(targetRatio).Forget();
        }
        // 你的 TriggerBufferSequence (負責計時與發號施令)
        private async UniTaskVoid DelayTriggerBufferSequence(Image bufferBar, float targetRatio, CancellationTokenSource bufferTokenSoure)
        {
            var token = _cts.Token;
            try
            {
                //等待緩衝時間
                await UniTask.Delay(500, cancellationToken: token);
                if (!bufferBar) return;
                // 延遲結束，發起動畫
                SmoothBufferUpdate(bufferBar, targetRatio, bufferTokenSoure).Forget();
            }
            catch (System.OperationCanceledException) { /* 正常取消 */ }
        }
        private CancellationTokenSource _hpBufferCts;

        private async UniTaskVoid SmoothBufferUpdate(Image bufferBar, float targetRatio, CancellationTokenSource bufferTokenSoure)
        {
            if (bufferBar == null) return;

            // 1. 直接操作類別成員變數，確保「外部」也能取消這個動畫
            _hpBufferCts?.Cancel();
            _hpBufferCts?.Dispose();
            _hpBufferCts = new CancellationTokenSource();
            var token = _hpBufferCts.Token;
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
