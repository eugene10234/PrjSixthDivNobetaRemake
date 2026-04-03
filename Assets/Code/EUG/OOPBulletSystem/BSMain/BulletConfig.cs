// ============================================================================
// Pure C# OOP Bullet System - 完整實現
// 團隊友好版：熟悉的 OOP + VContainer + EventBus + ObjectPool
// ============================================================================
using DG.Tweening;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
// ========================================================================
// BulletConfig (配置類)
// ========================================================================
namespace Nobeta.EUG.BulletSystem.OOP
{
    /// <summary>
    /// 子彈配置
    /// </summary>
    public class BulletConfig
    {


        public int Id { get; set; }
        public float Speed { get; set; }
        public int Damage { get; set; }
        public int ManaCost { get; set; }
        public float CoolTime { get; set; }
        //public bool CoolTimerLock { get; set; }
        public float Lifetime { get; set; }
        public float ColliderRadius { get; set; }
        public BulletType Type { get; private set; }
        public int ChantManaCost { get; set; }
        public float ChantDuration { get; set; }
        public bool ChantFlag { get; set; }
        public float ChantRatio { get; set; }

        public float NextFireTime { get; private set; }
        public void TryFire(Action FireAction)
        {
            if (Time.time < NextFireTime) return;
            FireAction?.Invoke();
            NextFireTime = Time.time + CoolTime;
        }
        public void TryChant(Action chantAction/*, Action KillChantAction*/)
        {
            if (ChantFlag) return;
            StartChant(chantAction);
        }
        public void StartChant(Action chantStartAction)
        {
            ChantRatio = 0f;
            ChantFlag = true;
            chantStartAction?.Invoke();
            AsyncChantProgess();
        }
        public void PauseChant(Action chantStopAction)
        {
            if (DOTween.IsTweening(this))
            {
                chantStopAction?.Invoke();
                DOTween.Kill(this);
            }
        }
        public void ContinueChant(Action chantContinueAction)
        {
            chantContinueAction?.Invoke();
            AsyncChantProgess();
        }
        public void KillChant()
        {
            PauseChant(ChantKillAction);
            ChantFlag = false;
        }
        private void AsyncChantProgess()
        {
            if (DOTween.IsTweening(this)) DOTween.Kill(this);
            DOTween.To(() => ChantRatio, x => ChantRatio = x, 1f, ChantDuration)
                          .SetEase(Ease.Linear)  // 均速增加
                          .SetUpdate(UpdateType.Normal, false)      // 受 TimeScale 影響
                          .SetId(this)
                          //.OnUpdate(() => 
                          //{
                          //      chantAction?.Invoke();
                          //})
                          .OnComplete(() =>
                          {
                              ChantCompleteAction?.Invoke();
                          });
        }
        private static Action ChantKillAction;
        private static Action ChantCompleteAction;
        public void SetChantCompleteAction(Action action)
        {
            ChantCompleteAction = action;
        }
        public void SetChantKillAction(Action action)
        {
            ChantKillAction = action;
        }
        // 預定義配置
        public static BulletConfig Normal => new BulletConfig
        {
            Speed = 35f,
            Damage = 40,
            ManaCost = 0,
            CoolTime = 0.5f,
            //CoolTimerLock = false,
            Lifetime = 5f,
            ColliderRadius = 0.2f,
            Type = BulletType.Normal,
            NextFireTime = Time.time,
            ChantManaCost = 200,
            ChantDuration = 5f,
            ChantFlag = false,
            ChantRatio = 0f,

        };
        public static BulletConfig IceBall => new BulletConfig
        {
            Speed = 45f,
            Damage = 25,
            ManaCost = 4,
            CoolTime = 0.15f,
            //CoolTimerLock = false,
            Lifetime = 5f,
            ColliderRadius = 0.15f,
            Type = BulletType.Ice,
            NextFireTime = Time.time,
            ChantManaCost = 20,
            ChantDuration = 5f,
            ChantFlag = false,
            ChantRatio = 0f,

        };
        public static BulletConfig FireBall => new BulletConfig
        {
            Speed = 15f,
            Damage = 30,
            ManaCost = 40,
            CoolTime = 1f,
            //CoolTimerLock = false,
            Lifetime = 4f,
            ColliderRadius = 0.3f,
            Type = BulletType.Fire,
            NextFireTime = Time.time,
            ChantManaCost = 20,
            ChantDuration = 5f,
            ChantFlag = false,
            ChantRatio = 0f,

        };
        public static BulletConfig LightningStrike => new BulletConfig
        {
            Speed = 80f,
            Damage = 250,
            ManaCost = 50,
            CoolTime = 1.25f,
            //CoolTimerLock = false,
            Lifetime = 4f,
            ColliderRadius = 0.1f,
            Type = BulletType.Lightning,
            NextFireTime = Time.time,
            ChantManaCost = 20,
            ChantDuration = 5f,
            ChantFlag = false,
            ChantRatio = 0f,

        };
    }

    public enum BulletType
    {
        Normal,
        Ice,
        Fire,
        Lightning
    }

}