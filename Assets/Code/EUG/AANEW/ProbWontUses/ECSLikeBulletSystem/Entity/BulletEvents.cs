using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
// 僅用於 Vector3, Quaternion 等數學類型
using UnityEngine;
// ========================================================================
// Events（事件 - Pure Struct）
// ========================================================================
namespace Nobeta.EUG.BulletSystemV1.ECSLike.Pure
{
    public struct BulletSpawnedEvent
    {
        public int bulletId;
        public Vector3 position;
        public BulletType type;
    }

    public struct BulletHitEvent
    {
        public int bulletId;
        public int targetId;
        public Vector3 hitPosition;
        public Vector3 hitNormal;
        public int damage;
        public BulletType type;
    }

    public struct BulletDestroyedEvent
    {
        public int bulletId;
        public Vector3 position;
        public BulletType type;
    }
}