using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code.EUG.MBFunctions.GUI
{
    public class StatusBarController : IDisposable
    {

        private Image _bufferImage;
        private RectTransform _bufferRectScaler;
        private CancellationTokenSource _cts;
        private CancellationTokenSource _animCts;

        public float FillAmount { get => _bufferImage.fillAmount; set => _bufferImage.fillAmount = value; }
        public StatusBarController()
        {
            StatusBarCtrlnitial();
        }
        public StatusBarController(Image bufferBar)
        {
            _bufferImage = bufferBar;
            StatusBarCtrlnitial();
        }
        public void ChangeFilter(Image currentMageCDFitler)
        {
            _bufferImage = currentMageCDFitler;
        }
        public StatusBarController(RectTransform bufferRect)
        {
            _bufferRectScaler = bufferRect;
            //var a = _bufferScaler.localScale;
            StatusBarCtrlnitial();
        }
        private void StatusBarCtrlnitial()
        {
            _cts = new CancellationTokenSource();
            FillType = new Property<float>();
            ColorType = new Property<Color>();
            RectType = new Property<Vector3>();
            PropertyInitial();
        }

        // 使用泛型版本
        public Property<float> FillType;
        public Property<Color> ColorType;
        public Property<Vector3> RectType;

        public void PropertyInitial()
        {
            // Float 版本：傳入 Mathf.Lerp
            FillType.Initial(
                () => _bufferImage.fillAmount,
                (val) => _bufferImage.fillAmount = val,
                (a, b, t) => Mathf.Lerp(a, b, t)
            );

            // Color 版本：傳入 Color.Lerp
            ColorType.Initial(
                () => _bufferImage.color,
                (val) => _bufferImage.color = val,
                (a, b, t) => Color.Lerp(a, b, t)
            );
            // Vector3 版本：傳入 Vector3.Lerp
            RectType.Initial(
                () => _bufferRectScaler.localScale,
                (val) => _bufferRectScaler.localScale = val,
                (a, b, t) => Vector3.Lerp(a, b, t)
            );
        }

        public void PropertyDispose()
        {
            FillType?.Dispose();
            ColorType?.Dispose();
        }

        public void Cleanup()
        {
            _cts?.Cancel();
            _animCts?.Cancel();
        }

        public void Dispose()
        {
            PropertyDispose();
            _cts?.Cancel();
            _animCts?.Cancel();
            _cts?.Dispose();
            _animCts?.Dispose();
        }


        /// <summary>
        /// 泛型化的 Property 類別，可以處理任何類型的動畫
        /// </summary>
        public class Property<T> : IDisposable
    {
        private Image _bufferBar;
        private CancellationTokenSource _animCts;
        private Func<T> _getFunc;
        private Action<T> _setAct;
        private Func<T, T, float, T> _lerpFunc;
        public Property()
        {
        }
        //public Property(Image bufferBar)
        //{
        //    _bufferBar = bufferBar;
        //}

        /// <summary>
        /// 初始化屬性
        /// </summary>
        /// <param name="getFunc">取得當前值的函數</param>
        /// <param name="setAct">設定新值的動作</param>
        /// <param name="lerpFunc">插值函數 (startValue, targetValue, t) => interpolatedValue</param>
        public void Initial(Func<T> getFunc, Action<T> setAct, Func<T, T, float, T> lerpFunc)
        {
            _getFunc = getFunc;
            _setAct = setAct;
            _lerpFunc = lerpFunc;
        }
        Func<float, float, float> EaseIn = (d, e) => Mathf.Pow((e / d), 2);
        Func<float, float, float> EaseOut = (d, e) => 1f - Mathf.Pow(1f - (e / d), 2);
        Func<float, float, float> LerpFunc = (d, e) => e / d;

        public void LerpUpdate(T target, float duration = 0.25f)
        {
            SmoothBufferUpdate(target, LerpFunc, duration).Forget();
        }

        public void EaseOutUpdate(T target, float duration = 0.25f)
        {
            SmoothBufferUpdate(target, EaseOut, duration).Forget();
        }
        public void EaseInUpdate(T target, float duration = 0.25f)
        {
            SmoothBufferUpdate(target, EaseIn, duration).Forget();
        }
        public async UniTaskVoid SmoothBufferUpdate(T target, Func<float, float, float> easingFunc, float duration = 0.5f)
        {
            if (_getFunc == null) return;
            if (_setAct == null) return;
            if (_lerpFunc == null) return;

            _animCts?.Cancel();
            _animCts?.Dispose();
            _animCts = new CancellationTokenSource();
            var token = _animCts.Token;
            T startValue = _getFunc.Invoke();
            float elapsed = 0f;

            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = easingFunc(duration, elapsed);
                    T currentValue = _lerpFunc(startValue, target, t);
                    _setAct.Invoke(currentValue);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
                _setAct.Invoke(target);
            }
            catch (System.OperationCanceledException) { }
        }

        public void Dispose()
        {
            _animCts?.Cancel();
            _animCts?.Dispose();
        }
        }


        public async UniTaskVoid DelayUpdateTrigger(float targetRatio, float duration = 0.5f)
        {
            try
            {
                await UniTask.Delay((int)(1000 * duration), cancellationToken: _cts.Token);
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

        public async UniTaskVoid SmoothBufferUpdate(float targetRatio, Func<float, float, float> easingFunc, float duration = 0.5f)
        {
            if (_bufferImage == null) return;

            _animCts?.Cancel();
            _animCts?.Dispose();
            _animCts = new CancellationTokenSource();
            var token = _animCts.Token;
            float startFill = _bufferImage.fillAmount;
            float elapsed = 0f;
            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = easingFunc(duration, elapsed);
                    _bufferImage.fillAmount = Mathf.Lerp(startFill, targetRatio, t);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
                _bufferImage.fillAmount = targetRatio;
            }
            catch (System.OperationCanceledException) { }
        }


    }
}