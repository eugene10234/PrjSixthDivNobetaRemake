using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
using UnityEngine;
using VContainer;
// ========================================================================
// Events (事件)
// ========================================================================
namespace Nobeta.EUG.BulletSystem.OOP
{
    public struct BulletSpawnedEvent
    {
        public Bullet Bullet;
        public Vector3 Position;
        public BulletType Type;
    }

    public struct BulletHitEvent
    {
        public Bullet Bullet;
        public GameObject TargetGameObject;
        public Vector3 HitPosition;
        public Vector3 HitNormal;
        public int Damage;
        public BulletType Type;
    }

    public struct BulletExpiredEvent
    {
        public Bullet Bullet;
        public Vector3 Position;
    }
}