using Assets.Code.EUG.MBFunctions.GUI;
using DG.Tweening;
using Nobeta.EUG.BulletSystemV2.OOP;
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
        [SerializeField] private Image chantBarColor;
        [SerializeField] private CanvasGroup chantBarGroup;
        [SerializeField] private CanvasGroup ChantSparkBarGroup;

        private IEventBus _eventBus;
        private CancellationTokenSource _cts;
        private StatusBarController _mgCDFilterCtrl;
        private MageIconHolder CurrentMageIconHolder;
        //private Image CurrentMageCDFitler;
        private Dictionary<BulletType, MageIconHolder> BulletTypeDiction = new();
        private void Awake()
        {
            _eventBus = EventBusProvider.GetEventBus();
        }
        private void OnEnable()
        {
            CurrentMageIconHolder = NormalIconHolder;
            //CurrentMageCDFitler = NormalIconHolder.GetFilter;
            _cts = new CancellationTokenSource();
            _mgCDFilterCtrl = new StatusBarController(CurrentMageIconHolder.GetFilter);
            chantBarGroup.alpha = 0f;
            // 訂閱事件
            SetDictionary();
            SubscribeEvents();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _mgCDFilterCtrl.Dispose();
            // 取消訂閱
            BulletTypeDiction.Clear();
            UnsubscribeEvents();
        }
        private void OnMagicChange(PlayerWeaponChangeEvent evt)
        {

            CurrentMageIconHolder.SetActice(false);
            CurrentMageIconHolder = BulletTypeDiction[evt.bulletConfig.Type];
            CurrentMageIconHolder.SetActice(true);
            _mgCDFilterCtrl.ChangeFilter(CurrentMageIconHolder.GetFilter);

            var tc = CurrentMageIconHolder.GetOriginColor;
            tc.a = 0.45f;
            chantBarColor.color = tc;

            if (evt.bulletConfig.CoolTime == 0) return;
            var UnProgressRatio = Mathf.Max(0, evt.bulletConfig.NextFireTime - Time.time) / evt.bulletConfig.CoolTime;
            MagicCoolTimeVisual(evt.bulletConfig.CoolTime, UnProgressRatio);
        }
        private void OnMagicFire(PlayerWeaponFireEvent evt)
        {
            MagicCoolTimeVisual(evt.bulletConfig.CoolTime, 1f);

        }
        private void MagicCoolTimeVisual(float coolTime, float unProgressRatio)
        {
            if (coolTime == 0) return;
            Color tc = CurrentMageIconHolder.GetOriginColor;
            tc.a = 1 - unProgressRatio;
            CurrentMageIconHolder.GetFilter.color = tc;
            tc.a = 1f;
            _mgCDFilterCtrl.FillAmount = 1 - unProgressRatio;

            _mgCDFilterCtrl.FillType.LerpUpdate(1f, coolTime * unProgressRatio);
            _mgCDFilterCtrl.ColorType.EaseInUpdate(tc, coolTime * unProgressRatio);
        }



        private void ChantBarVisualEnable()
        {
            if (chantBarGroup == null) return;
            chantBarGroup.alpha = 1f;
            //ChantBarFadeTween(1f);
        }
        private void ChantBarVisualDisable()
        {
            if (chantBarGroup == null) return;
            chantBarGroup.alpha = 0f;
            //ChantBarFadeTween(0f);
        }

        private void ChantBarFadeTween(float target)
        {
            if (DOTween.IsTweening(chantBarGroup)) DOTween.Kill(chantBarGroup);
            chantBarGroup.DOFade(target, 0.15f)
                .SetEase(Ease.OutQuad)
                .SetId(chantBarGroup);
        }

        private void OnChantStartVisual(PlayerChantStartEvent _)
        {
            if (chantBarColor == null) return;
            chantBarColor.fillAmount = 0f;
            ChantBarVisualEnable();
            //UpdateChantRatio(evt.bulletConfig.ChantRatio);
            //AsyncChantProgess(evt.bulletConfig.ChantDuration, 1 - evt.bulletConfig.ChantRatio);
        }
        private void OnChantKillVisual(PlayerChantKillEvent _)
        {
            if (chantBarColor == null) return;
            DOTween.Kill(chantBarColor);
            DOTween.Kill(ChantSparkBarGroup);
            chantBarColor.fillAmount = 0f;
            ChantSparkBarGroup.alpha = 0f;
            ChantBarVisualDisable();
        }
        //private void OnChantPauseVisual(PlayerChantPauseEvent evt)
        //{
        //    if (chantBarColor == null) return;
        //    PauseChantVisual();
        //}

        //private void PauseChantVisual()
        //{
        //    if (DOTween.IsTweening(chantBarColor))
        //    {
        //        DOTween.Kill(chantBarColor);
        //    }
        //}

        private void OnChantUpdateVisual(PlayerChantUpdateEvent evt)
        {
            if (chantBarColor == null) return;

            UpdateChantRatio(evt.TargetRatio);
            //chantBarColor.fillAmount = evt.TargetRatio * 0.25f;
            //AsyncChantProgess(evt.bulletConfig.ChantDuration, 1 - evt.bulletConfig.ChantRatio);
        }
        //private void AsyncChantProgess(float ChantDuration, float unProgressRatio)
        //{
        //    if (chantBarColor == null) return;
        //    if (DOTween.IsTweening(chantBarColor)) DOTween.Kill(chantBarColor);
        //    chantBarColor.fillAmount = (1 - unProgressRatio) * 0.25f;
        //    chantBarColor.DOFillAmount(0.25f, unProgressRatio * ChantDuration)
        //        .SetDelay(0.35f)
        //        .SetEase(Ease.Linear)
        //        .SetUpdate(UpdateType.Normal, false)
        //        .SetId(chantBarColor)
        //        .OnComplete(() =>
        //        {
        //            Debug.Log("計數完成！");
        //        });
        //}
        private void UpdateChantRatio(float targetRatio)
        {
            if (chantBarColor == null) return;

            // 1. 停止任何正在運行的動畫（避免衝突）
            if (DOTween.IsTweening(chantBarColor)) DOTween.Kill(chantBarColor);

            // 2. 直接計算目標值
            float finalAmount = targetRatio * 0.25f;

            // 3. 使用簡單的平滑移動，時間縮短到與事件頻率接近 (例如 0.2s)
            chantBarColor.DOFillAmount(finalAmount, 0.15f)
                .SetEase(Ease.Linear)
                .SetUpdate(UpdateType.Normal, false)
                .SetId(chantBarColor);
        }
        private void OnChantComplete(PlayerChantCompleteEvent _)
        {
            if (ChantSparkBarGroup == null) return;
            if (!DOTween.IsTweening(ChantSparkBarGroup))
            {
                var cIconSparker = CurrentMageIconHolder.GetIconSparker; // 獲取另一個圖示的 CanvasGroup

                // 1. 初始化狀態（確保兩者同步從 0 開始）
                ChantSparkBarGroup.alpha = 0f;
                if (cIconSparker != null) cIconSparker.alpha = 0f;

                // 2. 建立 Sequence
                Sequence mySeq = DOTween.Sequence();

                // 3. 使用 Join 讓兩個動畫同時啟動
                mySeq.Join(ChantSparkBarGroup.DOFade(1f, 0.15f))
                     .Join(cIconSparker.DOFade(1f, 0.15f)) // 如果 cIconSparker 是 null，DOTween 會安全忽略
                     .SetEase(Ease.Linear)
                     .SetLoops(-1, LoopType.Yoyo)
                     .SetUpdate(UpdateType.Normal, false) // 照你之前的需求，受 TimeScale 影響
                     .SetId(ChantSparkBarGroup); // 綁定 ID，讓 IsTweening 可以找到它
            }
        }
        private void SubscribeEvents()
        {
            _eventBus.Subscribe<PlayerWeaponFireEvent>(OnMagicFire);
            _eventBus.Subscribe<PlayerWeaponChangeEvent>(OnMagicChange);
            _eventBus.Subscribe<PlayerChantStartEvent>(OnChantStartVisual);
            _eventBus.Subscribe<PlayerChantUpdateEvent>(OnChantUpdateVisual);
            _eventBus.Subscribe<PlayerChantKillEvent>(OnChantKillVisual);
            _eventBus.Subscribe<PlayerChantCompleteEvent>(OnChantComplete);
        }

        private void UnsubscribeEvents()
        {
            _eventBus.Unsubscribe<PlayerWeaponFireEvent>(OnMagicFire);
            _eventBus.Unsubscribe<PlayerWeaponChangeEvent>(OnMagicChange);
            _eventBus.Unsubscribe<PlayerChantStartEvent>(OnChantStartVisual);
            _eventBus.Unsubscribe<PlayerChantUpdateEvent>(OnChantUpdateVisual);
            _eventBus.Unsubscribe<PlayerChantKillEvent>(OnChantKillVisual);
            _eventBus.Unsubscribe<PlayerChantCompleteEvent>(OnChantComplete);
        }
        //chantBarColor.DOFade(healthWarningPrototype, 0.25f) // 速度極快
        //            .SetEase(Ease.Linear)
        //            .SetLoops(-1, LoopType.Yoyo)
        //            .SetId("HPFlash");
        //DOTween.To(() => chantBarColor.color, x => ChantRatio = x, 1f, ChantDuration)
        //              .SetEase(Ease.Linear)  // 均速增加
        //              .SetUpdate(UpdateType.Normal, false)      // 受 TimeScale 影響
        //              .SetId(this)
        //              .OnUpdate(() => { })
        //              .OnComplete(() =>
        //              {
        //                  Debug.Log("計數完成！");
        //              });
        //}
        private void SetDictionary()
        {
            BulletTypeDiction[BulletType.Normal] = NormalIconHolder;
            BulletTypeDiction[BulletType.Ice] = IceIconHolder;
            BulletTypeDiction[BulletType.Fire] = FireIconHolder;
            BulletTypeDiction[BulletType.Lightning] = LightningIconHolder;
        }

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
