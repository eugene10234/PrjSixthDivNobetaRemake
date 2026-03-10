using Nobeta.BulletSystem.OOP;
using Nobeta.EUG.Interfaces;
using UnityEngine;
namespace Nobeta.EUG.AANEW.MBbulletsystems
{
    /// <summary>
    /// 子彈碰撞處理器
    /// 根據子彈類型發布不同事件
    /// </summary>
    public class BulletCollisionHandler : MonoBehaviour
    {
        private Bullet bullet;
        private IEventBus eventBus;

        private void Awake()
        {
            bullet = GetComponent<Bullet>();
        }

        private void Start()
        {
            eventBus = EventBusProvider.GetEventBus();
        }

        private void OnTriggerEnter(Collider other)
        {
            //if (!bullet.IsActive) return;


            HandleEnemyHit(other);

        }

        private void HandleEnemyHit(Collider hitObj)
        {
            //if (hitObj.CompareTag("Enemy") )
            //{
            //    eventBus?.Publish(new BulletHitEnemyEvent
            //    {
            //        Bullet = bullet,
            //        Enemy = hitObj.gameObject,
            //        Damage = bullet.Damage,
            //        HitPosition = transform.position
            //    });
            //}
            // 發布基礎擊中事件


            // 根據子彈類型發布特殊效果事件
            switch (bullet.Type)
            {
                case BulletType.Fire:
                    eventBus?.Publish(new ApplyBurnEffectEvent
                    {
                        Target = hitObj.gameObject,
                        Duration = 3f,
                        DamagePerSecond = 5
                    });
                    break;

                case BulletType.Ice:
                    eventBus?.Publish(new ApplySlowEffectEvent
                    {
                        Target = hitObj.gameObject,
                        Duration = 2f,
                        SlowPercent = 0.5f
                    });
                    break;

                case BulletType.Lightning:
                    eventBus?.Publish(new ApplyChainLightningEvent
                    {
                        Origin = hitObj.gameObject,
                        Damage = bullet.Damage / 2,
                        ChainCount = 3,
                        ChainRange = 5f
                    });
                    break;
            }
            Debug.Log($"Bullet of type {bullet.Type} hit {hitObj.name} at {transform.position}");
            // 歸還子彈
            bullet.ReturnToPool();
        }
    }

    // 特殊效果事件
    public struct ApplyBurnEffectEvent
    {
        public GameObject Target;
        public float Duration;
        public int DamagePerSecond;
    }

    public struct ApplySlowEffectEvent
    {
        public GameObject Target;
        public float Duration;
        public float SlowPercent;
    }

    public struct ApplyChainLightningEvent
    {
        public GameObject Origin;
        public int Damage;
        public int ChainCount;
        public float ChainRange;
    }
}