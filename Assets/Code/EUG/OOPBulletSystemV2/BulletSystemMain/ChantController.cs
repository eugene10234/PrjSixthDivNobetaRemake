using DG.Tweening;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using System;
using UnityEngine;

namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public class ChantController
    {
        private readonly IEventBus _eventBus;

        public bool ChantFlag { get; private set; }
        public bool IsChantComplete { get; private set; }
        public float ChantRatio { get; private set; }

        // Config 傳進來，不自己持有
        private float _chantDuration;
        private float _lastInvokeTime;

        public ChantController(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void SetDuration(float duration)
        {
            _chantDuration = duration;
        }

        public void TryChant()
        {
            if (ChantFlag) return;
            StartChant();
        }

        public void StartChant()
        {
            ChantRatio = 0f;
            ChantFlag = true;
            _eventBus.Publish(new PlayerChantStartEvent { });
            RunChantAsync();
        }

        public void PauseChant()
        {
            if (DOTween.IsTweening(this)) DOTween.Kill(this);
        }

        public void ContinueChant()
        {
            RunChantAsync();
        }

        public void KillChant()
        {
            PauseChant();
            _eventBus.Publish(new PlayerChantKillEvent { });
            ChantFlag = false;
        }

        public void Reset()
        {
            ChantRatio = 0f;
            IsChantComplete = false;
            ChantFlag = false;
            KillChant();
        }

        private void RunChantAsync()
        {
            if (DOTween.IsTweening(this)) DOTween.Kill(this);

            DOTween.To(() => ChantRatio, x => ChantRatio = x, 1f, _chantDuration * (1 - ChantRatio))
                   .SetEase(Ease.Linear)
                   .SetUpdate(UpdateType.Normal, false)
                   .SetId(this)
                   .OnStart(() => { _lastInvokeTime = 0f; })
                   .OnUpdate(() =>
                   {
                       if (Time.time - _lastInvokeTime >= 0.05f)
                       {
                           _eventBus.Publish(new PlayerChantUpdateEvent { TargetRatio = ChantRatio });
                           _lastInvokeTime = Time.time;
                       }
                   })
                   .OnComplete(() =>
                   {
                       IsChantComplete = true;
                       _eventBus.Publish(new PlayerChantCompleteEvent { });
                   });
        }
    }
}
