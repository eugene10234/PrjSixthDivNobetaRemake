//using Nobeta.EUG.Core.Events;
using UnityEngine;
// ========================================================================
// PhysicsService (物理服務實現)
// ========================================================================
namespace Nobeta.EUG.BulletSystemV2.OOP.UnityBridge
{

    /// <summary>
    /// 物理服務（封裝 Unity Physics）
    /// </summary>
    public class PhysicsService : IPhysicsService
    {
        public CollisionHit OverlapSphere(Vector3 position, float radius, Vector3 direction, int layerMask)
        {
            // 1. 使用 NonAlloc 避免產生垃圾記憶體 (效能優化關鍵)
            Collider[] results = new Collider[5];
            int count = Physics.OverlapSphereNonAlloc(position, radius, results, layerMask);

            for (int i = 0; i < count; i++)
            {
                Collider hitCollider = results[i];

                // 2. 取得離子彈最近的碰撞點與法線
                Vector3 closestPoint = hitCollider.ClosestPoint(position);
                Vector3 normal = (position - closestPoint).normalized;

                // 如果距離太近導致 normal 算不出來(Zero)，就跳過
                if (normal == Vector3.zero) normal = -direction;

                // 3. 判斷角度 (假設你想過濾掉平行的地面)
                float angle = Vector3.Angle(normal, -direction);

                // 如果是環境層級且夾角太斜，我們就當作沒撞到，繼續檢查下一個
                bool isEnvironment = ((1 << hitCollider.gameObject.layer) & LayerMask.GetMask("Ground", "Obstacle")) != 0;

                if (isEnvironment && angle > 60f) // 舉例：大於 60 度太斜了，無視
                {
                    continue;
                }

                // 找到第一個符合條件的就回傳
                return new CollisionHit
                {
                    HasHit = true,
                    GameObject = hitCollider.gameObject,
                    Normal = normal,
                    Point = closestPoint // 建議 CollisionHit 增加這個欄位
                };
            }

            return new CollisionHit { HasHit = false };
        }
        public CollisionHit SphereCast(Vector3 position, float radius, Vector3 direction, float distance, int layerMask)
        {
            RaycastHit hit;

            // 執行 SphereCast
            if (Physics.SphereCast(position, radius, direction, out hit, distance, layerMask))
            {
                // 取得地面和障礙物的 LayerMask
                int environmentMask = LayerMask.GetMask("Ground", "Obstacle");
                bool isEnvironment = ((1 << hit.collider.gameObject.layer) & environmentMask) != 0;

                if (isEnvironment)
                {
                    // 計算子彈方向與碰撞面法線的夾角
                    // Vector3.Angle 會回傳 0~180 度
                    // 當子彈正對牆壁撞擊時，角度接近 180；當子彈與地面平行擦過時，角度接近 90
                    float impactAngle = Vector3.Angle(direction, hit.normal);

                    // 邏輯：如果角度在 120~180 之間，代表是較為垂直的撞擊 (有效)
                    // 如果角度在 90~120 之間，代表是嚴重的擦邊 (無效，過濾掉)
                    // 推薦閾值：150 度 (相當於法線與子彈反方向夾角 30 度)
                    if (impactAngle < 150f)
                    {
                        // 如果太過平行地板，我們回傳 HasHit = false，讓子彈穿過去
                        return new CollisionHit { HasHit = false };
                    }
                }
                //IDamageable target;
                //if (DamageSystem.TryGetTarget(hits[0], out target))
                //{
                //    target.TakeDamage(50, position);
                //    // AudioClip clip = Resources.Load<AudioClip>($"Audio/SFX/{sfxName}");
                //    // AudioSource.PlayClipAtPoint(clip, Vector3.zero, volume);Assets/MasterMagicFX/ParticlesVer3/Cosmos/CosmoShoot2/Prefabs/Par_CosmoShootMuzzle.prefab

                //}
                // 符合條件（或者是敵人），回傳正式的命中資料
                return new CollisionHit
                {
                    HasHit = true,
                    GameObject = hit.collider.gameObject,
                    Normal = hit.normal,
                    Point = hit.point,
                    //Distance = hit.distance,
                    //Layer = hit.collider.gameObject.layer
                };
            }
            return new CollisionHit { HasHit = false };
        }
    }
}


