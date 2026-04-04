using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
using UnityEngine;
// 僅用於 Vector3, Quaternion 等數學類型
using VContainer;
// ========================================================================
// Weapon System（使用示例）
// ========================================================================
namespace Nobeta.EUG.BulletSystemV1.ECSLike.Pure
{
    /// <summary>
    /// 武器系統（使用 BulletManager 的示例）
    /// </summary>
    public class WeaponSystemPure : MonoBehaviour
    {
        [SerializeField] private BulletManagerBridge bulletManager;
        [SerializeField] private Transform firePoint;

        [Inject] private IPlayerInputService inputService;

        private void Start()
        {
            //inputService.OnFireEnd += Fire;
        }

        private void OnDestroy()
        {
            //inputService.OnFireEnd -= Fire;
        }

        private void Fire()
        {
            bulletManager.Fire(
                BulletManagerConfig.NormalBullet,
                firePoint.position,
                firePoint.forward
            );
        }
    }

}