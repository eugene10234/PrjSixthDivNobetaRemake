using System.Collections.Generic;
using Nobeta.EUG.BulletSystem.OOP;
//using Nobeta.EUG.Core.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using Unity.Mathematics;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.EUG.BulletSystem.OOP.UnityBridge
{
    // ========================================================================
    // Usage Example (使用示例)
    // ========================================================================

    /// <summary>
    /// 武器系統（使用示例）
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        [SerializeField] private BulletSystemBridge bulletSystem;
        [SerializeField] private Transform firePoint;

        [Inject] private IPlayerInputService inputService;

        private void Start()
        {
            for (int i = 0; i < 80; i++)
            {
                inputService.OnFireStart += Fire;
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < 80; i++)
            {
                inputService.OnFireStart -= Fire;
            }
        }

        private void Fire()
        {
            bulletSystem.Fire(
                BulletConfig.Normal,
                firePoint.position + UnityEngine.Random.Range(-20f, 20f) * firePoint.up + UnityEngine.Random.Range(-20f, 20f) * firePoint.right,
                firePoint.forward
            );
        }
    }
}


