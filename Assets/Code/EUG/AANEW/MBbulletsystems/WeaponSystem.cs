using UnityEngine;
using VContainer;
using Nobeta.EUG.Core.Pooling;
using Nobeta.EUG.Interfaces;
using System;

namespace Nobeta.EUG.AANEW.MBbulletsystems
{
    /// <summary>
    /// 武器系統
    /// 使用單一池 + 配置切換
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        [Header("子彈設置")]
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private Transform firePoint;

        [Header("子彈配置")]
        [SerializeField] private BulletConfig fireBallConfig;
        [SerializeField] private BulletConfig iceBallConfig;
        [SerializeField] private BulletConfig lightningBallConfig;

        private Pool<Bullet> bulletPool;
        private BulletConfig currentBulletConfig;

        [Inject] private IEventBus eventBus;

        #region Initialization

        private void Awake()
        {
            if (eventBus == null)
            {
                Debug.LogError("WeaponSystem: 無法獲取 IEventBus 依賴！");
                return;
            }
            // 只創建一個池！
            bulletPool = new Pool<Bullet>(bulletPrefab, new PoolConfig
            {
                InitialCapacity = 50,
                PrewarmCount = 20,
                MaxCapacity = 100
            });
            eventBus.Subscribe<WeaponFiredEvent>(OnFire);
            // 默認使用火球
            currentBulletConfig = fireBallConfig;
        }

        private void OnFire(WeaponFiredEvent @event)
        {

            //var bullet = bulletPool.Get();
            Debug.LogWarning($"{DateTime.UtcNow}子彈OnFire");
            //if (bullet == null)
            //{
            //    Debug.LogWarning("子彈池耗盡！");
            //    return;
            //}

            //// 使用當前配置初始化
            //bullet.Initialize(currentBulletConfig);

            //// 設置位置
            //bullet.transform.position = @event.Position;
            //bullet.transform.rotation = firePoint.rotation;

            //// 設置池引用
            //bullet.SetPool(bulletPool);

            //// 發射
            //bullet.Fire(@event.Forward);
        }

        #endregion

        #region Weapon Switching

        public void SwitchToFireBall()
        {
            currentBulletConfig = fireBallConfig;
            Debug.Log("切換到火球");
        }

        public void SwitchToIceBall()
        {
            currentBulletConfig = iceBallConfig;
            Debug.Log("切換到冰球");
        }

        public void SwitchToLightningBall()
        {
            currentBulletConfig = lightningBallConfig;
            Debug.Log("切換到雷電球");
        }

        #endregion

        #region Firing

        //public void Fire()
        //{
        //    // 從池獲取通用子彈
        //    var bullet = bulletPool.Get();

        //    if (bullet == null)
        //    {
        //        Debug.LogWarning("子彈池耗盡！");
        //        return;
        //    }

        //    // 使用當前配置初始化
        //    bullet.Initialize(currentBulletConfig);

        //    // 設置位置
        //    bullet.transform.position = firePoint.position;
        //    bullet.transform.rotation = firePoint.rotation;

        //    // 設置池引用
        //    bullet.SetPool(bulletPool);

        //    // 發射
        //    bullet.Fire(firePoint.forward);

        //    // 發布事件
        //    eventBus.Publish(new WeaponFiredEvent
        //    {
        //        BulletType = currentBulletConfig.bulletType,
        //        Position = firePoint.position
        //    });
        //}

        #endregion
        private void OnGUI()
        {
            if (false)
            {
                DrawDebugInfo();
            }

        }
        private void DrawDebugInfo()
        {

            if (!Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));

            if (GUILayout.Button("Fire"))
            {
                eventBus.Publish(new WeaponFiredEvent
                {
                    BulletType = currentBulletConfig.bulletType,
                    Position = firePoint.position
                });
            }
            if (GUILayout.Button("SwitchToFireBall"))
            {
                SwitchToFireBall();
            }
            if (GUILayout.Button("SwitchToIceBall"))
            {
                SwitchToIceBall();
            }
            if (GUILayout.Button("SwitchToLightningBall"))
            {
                SwitchToLightningBall();
            }
            GUILayout.EndArea();

        }
    }
}
