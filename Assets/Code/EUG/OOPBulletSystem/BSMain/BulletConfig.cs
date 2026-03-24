// ============================================================================
// Pure C# OOP Bullet System - 完整實現
// 團隊友好版：熟悉的 OOP + VContainer + EventBus + ObjectPool
// ============================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using UnityEngine;
using VContainer;
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
        public float NextFireTime { get; private set; }
        public void TryFire(Action FireAction)
        {
            if (Time.time < NextFireTime) return;
            FireAction?.Invoke();
            NextFireTime = Time.time + CoolTime;
        }

        // 預定義配置
        public static BulletConfig Normal => new BulletConfig
        {
            Speed = 20f,
            Damage = 40,
            ManaCost = 0,
            CoolTime = 0.5f,
            //CoolTimerLock = false,
            Lifetime = 5f,
            ColliderRadius = 0.1f,
            Type = BulletType.Lightning,
            NextFireTime = Time.time

        };
        public static BulletConfig IceBall => new BulletConfig
        {
            Speed = 20f,
            Damage = 25,
            ManaCost = 0,
            CoolTime = 0.15f,
            //CoolTimerLock = false,
            Lifetime = 5f,
            ColliderRadius = 0.1f,
            Type = BulletType.Ice,
            NextFireTime = Time.time

        };
        public static BulletConfig FireBall => new BulletConfig
        {
            Speed = 15f,
            Damage = 20,
            ManaCost = 0,
            CoolTime = 1f,
            //CoolTimerLock = false,
            Lifetime = 4f,
            ColliderRadius = 0.2f,
            Type = BulletType.Fire,
            NextFireTime = Time.time

        };
        public static BulletConfig LightningStrike => new BulletConfig
        {
            Speed = 15f,
            Damage = 20,
            ManaCost = 0,
            CoolTime = 1f,
            //CoolTimerLock = false,
            Lifetime = 4f,
            ColliderRadius = 0.2f,
            Type = BulletType.Normal,
            NextFireTime = Time.time

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