using UnityEngine;
// ============================================================================
// BulletEvents v2  ── BulletHitEvent 加入 HitType，讓 VFX 知道要播哪種特效
// ============================================================================
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public struct BulletExpiredEvent
    {
        public BulletState BulletState;
        public Vector3 Position;
    }
}