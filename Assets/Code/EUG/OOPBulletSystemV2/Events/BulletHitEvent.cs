using UnityEngine;
// ============================================================================
// BulletEvents v2  ── BulletHitEvent 加入 HitType，讓 VFX 知道要播哪種特效
// ============================================================================
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public struct BulletHitEvent
    {
        public Bullet Bullet; 
        public BulletState BulletState; 
        public GameObject TargetGameObject;
        public Vector3 HitPosition;
        public Vector3 HitNormal;
        public int Damage;
        public BulletType Type;
        public BulletHitType HitType;
        public float AoeRadius;
    }
}