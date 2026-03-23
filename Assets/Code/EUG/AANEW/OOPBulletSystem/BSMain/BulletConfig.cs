// ============================================================================
// Pure C# OOP Bullet System - 完整實現
// 團隊友好版：熟悉的 OOP + VContainer + EventBus + ObjectPool
// ============================================================================
using System;
using System.Collections.Generic;
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
        public bool CoolTimerLock { get; set; }
        public float Lifetime { get; set; }
        public float ColliderRadius { get; set; }
        public BulletType Type { get; set; }

        // 預定義配置
        public static BulletConfig Normal => new BulletConfig
        {
            Speed = 20f,
            Damage = 10,
            ManaCost= 10,
            CoolTime = 0.5f,
            CoolTimerLock = false,
            Lifetime = 5f,
            ColliderRadius = 0.1f,
            Type = BulletType.Normal
        };
        public static BulletConfig IceBall => new BulletConfig
        {
            Speed = 20f,
            Damage = 5,
            ManaCost = 5,
            CoolTime = 0.15f,
            CoolTimerLock = false,
            Lifetime = 5f,
            ColliderRadius = 0.1f,
            Type = BulletType.Ice
        };
        public static BulletConfig FireBall => new BulletConfig
        {
            Speed = 15f,
            Damage = 20,
            ManaCost = 25,
            CoolTime = 1f,
            CoolTimerLock = false,
            Lifetime = 4f,
            ColliderRadius = 0.2f,
            Type = BulletType.Fire
        };
        public static BulletConfig LightningStrike => new BulletConfig
        {
            Speed = 15f,
            Damage = 20,
            ManaCost = 5,
            CoolTime = 1f,
            CoolTimerLock = false,
            Lifetime = 4f,
            ColliderRadius = 0.2f,
            Type = BulletType.Fire
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