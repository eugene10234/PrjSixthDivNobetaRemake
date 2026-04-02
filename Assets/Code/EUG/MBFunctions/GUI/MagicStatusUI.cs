using Assets.Code.EUG.MBFunctions.GUI;
using DG.Tweening;
using Nobeta.EUG.BulletSystem.OOP;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
// using TMPro; // 如果你有用到文字

namespace Nobeta.EUG.MBFunctions.PlayerUI
{
    public class MagicStatusUI : MonoBehaviour
    {
        [Header("UI References")]
        //[SerializeField] private Image OLDMageCDBuffer;
        [SerializeField] private MageIconHolder NormalIconHolder;
        [SerializeField] private MageIconHolder IceIconHolder;
        [SerializeField] private MageIconHolder FireIconHolder;
        [SerializeField] private MageIconHolder LightningIconHolder;
        [Inject] private readonly IEventBus _eventBus;
        private CancellationTokenSource _cts;
        private StatusBarController _mgCDFilterCtrl;
        private MageIconHolder CurrentMageIconHolder;
        //private Image CurrentMageCDFitler;
        private Dictionary<BulletType, MageIconHolder> BulletTypeDiction = new();
        private void OnEnable()
        {
            CurrentMageIconHolder = NormalIconHolder;
            //CurrentMageCDFitler = NormalIconHolder.GetFilter;
            _cts = new CancellationTokenSource();
            _mgCDFilterCtrl = new StatusBarController(CurrentMageIconHolder.GetFilter);
            // 訂閱事件
            SetDictionary();
            _eventBus.Subscribe<PlayerWeaponFireEvent>(OnMagicFire);
            _eventBus.Subscribe<PlayerWeaponChangeEvent>(OnMagicChange);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _mgCDFilterCtrl.Dispose();
            // 取消訂閱
            BulletTypeDiction.Clear();
            _eventBus.Unsubscribe<PlayerWeaponFireEvent>(OnMagicFire);
            _eventBus.Unsubscribe<PlayerWeaponChangeEvent>(OnMagicChange);
        }
        private void OnMagicChange(PlayerWeaponChangeEvent evt)
        {

            CurrentMageIconHolder.SetActice(false);
            CurrentMageIconHolder = BulletTypeDiction[evt.bulletConfig.Type];
            CurrentMageIconHolder.SetActice(true);
            _mgCDFilterCtrl.ChangeFilter(CurrentMageIconHolder.GetFilter);

            if (evt.bulletConfig.CoolTime == 0) return;
            var UnProgressRatio =Mathf.Max(0, evt.bulletConfig.NextFireTime - Time.time) / evt.bulletConfig.CoolTime;
            MagicCoolTimeVisual(evt.bulletConfig.CoolTime, UnProgressRatio);
        }
        private void OnMagicFire(PlayerWeaponFireEvent evt)
        {
            MagicCoolTimeVisual(evt.bulletConfig.CoolTime, 1f);

        }
        private void MagicCoolTimeVisual(float coolTime, float unProgressRatio)
        {
            if (coolTime == 0) return;
            var tc = CurrentMageIconHolder.GetOriginColor;
            tc.a = 1 - unProgressRatio;
            CurrentMageIconHolder.GetFilter.color = tc;
            tc.a = 1f;
            _mgCDFilterCtrl.FillAmount = 1 - unProgressRatio;

            _mgCDFilterCtrl.FillType.LerpUpdate(1f, coolTime * unProgressRatio);
            _mgCDFilterCtrl.ColorType.EaseInUpdate(tc, coolTime * unProgressRatio);
        }
        private void SetDictionary()
        {
            BulletTypeDiction[BulletType.Normal] = NormalIconHolder;
            BulletTypeDiction[BulletType.Ice] = IceIconHolder;
            BulletTypeDiction[BulletType.Fire] = FireIconHolder;
            BulletTypeDiction[BulletType.Lightning] = LightningIconHolder;
        }
        //private CancellationTokenSource MagicCoolTimeTokenSoure;
        //private void UpdateBarBufferWithDelay(Image bufferBar, float duration, CancellationTokenSource bufferTokenSoure)
        //{
        //    bufferBar.fillAmount = 0f;
        //    SmoothBufferUpdate(bufferBar, 1f, duration, MagicCoolTimeTokenSoure).Forget();
        //}

        //private async UniTaskVoid SmoothBufferUpdate(Image bufferBar, float targetRatio, float duration, CancellationTokenSource bufferTokenSoure)
        //{
        //    // 這裡維持你的邏輯：殺掉正在跑的舊動畫，但不影響 PRXY 的 Delay
        //    bufferTokenSoure?.Cancel();
        //    bufferTokenSoure?.Dispose();
        //    bufferTokenSoure = new CancellationTokenSource();
        //    var token = bufferTokenSoure.Token;
        //    float startFill = bufferBar.fillAmount;
        //    float elapsed = 0f;
        //    try
        //    {
        //        while (elapsed < duration)
        //        {
        //            elapsed += Time.deltaTime;
        //            //1f - Mathf.Pow(1f - (elapsed / duration), 2);
        //            float t = elapsed / duration;

        //            bufferBar.fillAmount = Mathf.Lerp(startFill, targetRatio, t);
        //            await UniTask.Yield(PlayerLoopTiming.Update, token);
        //        }
        //        bufferBar.fillAmount = targetRatio;
        //    }
        //    catch (System.OperationCanceledException) { }
        //}
        private CanvasGroup canvasGroup;
        private float _fadeDuration = 0.5f;
        // 淡入
        public void FadeIn()
        {
            // 從目前的透明度變化到 1
            canvasGroup.DOFade(1, _fadeDuration);
        }

        // 淡出
        public void FadeOut()
        {
            // 變化到 0
            canvasGroup.DOFade(0, _fadeDuration);
        }
        public void FadeInAndEnable()
        {
            // 從目前的透明度變化到 1
            canvasGroup.DOFade(1, _fadeDuration).OnPlay(() =>
            {
                gameObject.SetActive(true); // 動畫開始時啟用物件
                Debug.Log("Fade In Started!");
            });
        }
        public void FadeOutAndDisable()
        {
            canvasGroup.DOFade(0, _fadeDuration).OnComplete(() =>
            {
                gameObject.SetActive(false); // 動畫結束後關閉物件
                Debug.Log("Fade Out Finished!");
            });
        }
        public void FadeInWithDelay()
        {
            // 延遲 3 秒後，啟動耗時 1 秒的淡入動畫
            canvasGroup.DOFade(1, 1f).SetDelay(3f);
        }
        public void SequenceFade()
        {
            Sequence mySequence = DOTween.Sequence();

            mySequence.AppendInterval(3f)     // 插入 3 秒空白等待
                      .Append(canvasGroup.DOFade(1, 1f)) // 然後淡入
                      .OnComplete(() => Debug.Log("延遲淡入完成"));
        }
    }
}
