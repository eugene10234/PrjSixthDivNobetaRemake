using DG.Tweening;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
// using TMPro; // 如果你有用到文字

namespace Nobeta.EUG.MBFunctions.PlayerUI
{
    public class BossInfoUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image bossHealthBar;
        [SerializeField] private Image bossHealthBuffer;
        private IEventBus _eventBus;
        private float _fadeDuration = 0.5f;
        private void Awake()
        {
            canvasGroup.gameObject.SetActive(false);
            _eventBus = EventBusProvider.GetEventBus();
        }
        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }
        private void OnBossHealthUpdate(BossHealthUpdateEvent evt)
        {
            float targetRatio = evt.HealthRatio;
            UpdateBossHealth(targetRatio);
        }
        private void OnBossArenaStart(BossInfoEnableEvent evt)
        {
            FadeInAndEnable();
            float targetRatio = evt.HealthRatio;
            bossHealthBar.fillAmount = 0f;
            bossHealthBuffer.fillAmount = 0f;
            UpdateBossHealth(targetRatio);
        }
        private void OnBossArenaEnd(BossInfoDisableEvent evt)
        {
            FadeOutAndDisable();
            bossHealthBar.fillAmount = 0f;
            bossHealthBuffer.fillAmount = 0f;
        }
        private void UpdateBossHealth(float targetRatio)
        {
            bossHealthBar.fillAmount = targetRatio;
            DOVirtual.DelayedCall(0.5f, () =>
            {
                if (!DOTween.IsTweening(bossHealthBuffer)) DOTween.Kill(bossHealthBuffer);
                bossHealthBuffer.DOFillAmount(targetRatio, 0.5f)
                              .SetEase(Ease.OutQuad)
                              .SetId(bossHealthBuffer);
            });
        }
        public void FadeInAndEnable()
        {
            canvasGroup.DOFade(1, _fadeDuration).OnPlay(() =>
            {
                // 動畫開始時啟用物件
                canvasGroup.gameObject.SetActive(true);
            });
        }
        public void FadeOutAndDisable()
        {
            canvasGroup.DOFade(0, _fadeDuration).OnComplete(() =>
            {
                // 動畫結束後關閉物件
                canvasGroup.gameObject.SetActive(false);
            });
        }
        private void SubscribeEvents()
        {
            _eventBus.Subscribe<BossInfoEnableEvent>(OnBossArenaStart);
            _eventBus.Subscribe<BossInfoDisableEvent>(OnBossArenaEnd);
            _eventBus.Subscribe<BossHealthUpdateEvent>(OnBossHealthUpdate);
        }

        private void UnsubscribeEvents()
        {
            _eventBus.Unsubscribe<BossInfoEnableEvent>(OnBossArenaStart);
            _eventBus.Unsubscribe<BossInfoDisableEvent>(OnBossArenaEnd);
            _eventBus.Subscribe<BossHealthUpdateEvent>(OnBossHealthUpdate);
        }

    }
}
