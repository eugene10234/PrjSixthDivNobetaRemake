using UnityEngine;
namespace Nobeta.EUG.AANEW.MBbulletsystems
{
    /// <summary>
    /// 子彈擊中敵人事件
    /// </summary>
    public struct BulletHitEnemyEvent
    {
        public Bullet Bullet;
        public GameObject Enemy;
        public int Damage;
        public Vector3 HitPosition;
        public Vector3 HitNormal;
    }
}