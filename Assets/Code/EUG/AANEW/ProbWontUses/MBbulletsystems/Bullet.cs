using UnityEngine;
using Nobeta.EUG.Core.Pooling;
using System;

namespace Nobeta.EUG.AANEW.MBbulletsystems
{
    /// <summary>
    /// 通用子彈組件
    /// 通過配置數據驅動不同類型
    /// </summary>
    public class Bullet : MonoBehaviour, IPoolable
    {
        #region Components

        private Rigidbody rb;
        private TrailRenderer trail;
        private Transform visualContainer;

        #endregion

        #region Runtime Data

        private BulletConfig currentConfig;
        private GameObject currentVisual;
        private Vector3 direction;
        public float currentLifetime;
        private bool isActive;
        private Pool<Bullet> pool;

        #endregion

        #region Properties

        public BulletConfig Config => currentConfig;
        public int Damage => currentConfig?.damage ?? 0;
        public BulletType Type => currentConfig?.bulletType ?? BulletType.Normal;

        public bool IsActive { get => isActive; internal set => isActive = value; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            trail = GetComponent<TrailRenderer>();

            // 創建視覺容器
            visualContainer = new GameObject("Visual").transform;
            visualContainer.SetParent(transform);
            visualContainer.localPosition = Vector3.zero;
        }

        private void Update()
        {
            if (!isActive) return;

            currentLifetime -= Time.deltaTime;
            if (currentLifetime <= 0)
            {
                ReturnToPool();
            }
        }

        private void FixedUpdate()
        {
            if (!isActive) return;
            rb.linearVelocity = direction * currentConfig.speed;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化子彈（使用配置）
        /// </summary>
        public void Initialize(BulletConfig config)
        {
            currentConfig = config;

            // 切換視覺
            SwitchVisual(config.visualPrefab);

            // 設置拖尾
            if (trail != null)
            {
                trail.startColor = config.trailColor;
                trail.endColor = new Color(config.trailColor.r, config.trailColor.g, config.trailColor.b, 0);
                trail.startWidth = config.trailWidth;
            }
        }

        /// <summary>
        /// 切換視覺效果
        /// </summary>
        private void SwitchVisual(GameObject visualPrefab)
        {
            // 清理舊視覺
            if (currentVisual != null)
            {
                // Debug.LogWarning($"{DateTime.UtcNow}Destroy");
                Destroy(currentVisual);
            }

            // 創建新視覺
            if (visualPrefab != null)
            {
                // Debug.LogWarning($"{DateTime.UtcNow}Instantiate");
                currentVisual = Instantiate(visualPrefab, visualContainer);
                currentVisual.transform.localPosition = Vector3.zero;
                currentVisual.transform.localRotation = Quaternion.identity;
            }
        }

        #endregion

        #region IPoolable

        public void OnSpawn()
        {
            isActive = true;
            currentLifetime = currentConfig?.lifetime ?? 5f;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (trail != null)
            {
                trail.Clear();
            }
        }

        public void OnDespawn()
        {
            isActive = false;
            direction = Vector3.zero;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        #endregion

        #region Public Methods

        public void Fire(Vector3 fireDirection)
        {
            direction = fireDirection.normalized;

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        public void SetPool(Pool<Bullet> bulletPool)
        {
            pool = bulletPool;
        }

        public void ReturnToPool()
        {
            pool?.Return(this);
        }

        #endregion
    }
}