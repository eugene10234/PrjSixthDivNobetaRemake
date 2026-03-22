using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code.EUG.MBFunctions.GUI
{
    public class StatusBarController:IDisposable
    {
        private Image _bufferBar;
        private CancellationTokenSource _cts;
        private CancellationTokenSource _animCts;

        public float FillAmount { get =>_bufferBar.fillAmount;  set => _bufferBar.fillAmount = value; }

        public StatusBarController(Image bufferBar)
        {
            _bufferBar = bufferBar;
            _cts = new CancellationTokenSource();
        }

        // 這裡放入你那套完美的「雙 Token 邏輯」
        public async UniTaskVoid DelayUpdateTrigger(float targetRatio, float duration = 0.5f)
        {
            // 管理 Delay Token
            try
            {
                await UniTask.Delay((int)(1000* duration), cancellationToken: _cts.Token);

                SmoothBufferUpdate(targetRatio, EaseOut).Forget();
            }
            catch (System.OperationCanceledException) { }
        }
        Func<float, float, float> EaseOut = (d, e) => 1f - Mathf.Pow(1f - (e / d), 2);
        Func<float, float, float> LerpFunc = (d, e) => e / d;
        public void LerpUpdate(float targetRatio, float duration = 0.5f)
        {
            SmoothBufferUpdate(targetRatio, LerpFunc, duration).Forget();
        }
        public async UniTaskVoid SmoothBufferUpdate(float targetRatio, Func<float, float, float> easingFunc, float duration = 0.5f )
        {
            if (_bufferBar == null) return;

            // 1. 直接操作類別成員變數，確保「外部」也能取消這個動畫
            _animCts?.Cancel();
            _animCts?.Dispose();
            _animCts = new CancellationTokenSource();
            var token = _animCts.Token;
            float startFill = _bufferBar.fillAmount;
            float elapsed = 0f;
            try
            {
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = easingFunc(duration, elapsed);

                    _bufferBar.fillAmount = Mathf.Lerp(startFill, targetRatio, t);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
                _bufferBar.fillAmount = targetRatio;
            }
            catch (System.OperationCanceledException) { }
        }
        
        private static float NewMethod(float duration, float elapsed)
        {
            return 1f - Mathf.Pow(1f - (elapsed / duration), 2);
        }

        public void Cleanup()
        {
            _cts?.Cancel();
            _animCts?.Cancel();
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _animCts?.Cancel();
            _cts?.Dispose();
            _animCts?.Dispose();
        }
    }
}
