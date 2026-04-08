using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VContainer;

// ============================================================================
// BulletSystemBridge v2  ── 唯一的 MonoBehaviour
//
// 主要變更：
//   - OnBulletHit 接收完整 BulletHitEvent（含 HitType）
//   - PlayHitEffectAsync 改傳 BulletHitEvent（而非 int type）
//   - 新增 FireWithStrategy() 讓外部直接用 config 的 strategy 發射
//   - RegisterIntervalFire / CancelIntervalFire
// ============================================================================
namespace Nobeta.EUG.BulletSystemV2.OOP.UnityBridge
{
    public interface IBulletSystem
    {
        public List<Bullet> Fire(BulletConfig config, Vector3 position, Vector3 direction);
    }
}
