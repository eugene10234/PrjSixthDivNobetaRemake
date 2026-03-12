using System.Collections.Generic;
using Nobeta.EUG.BulletSystem.OOP;
//using Nobeta.EUG.Core.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.EUG.BulletSystem.UnityBridge
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
            inputService.OnFireStart += Fire;
        }

        private void OnDestroy()
        {
            inputService.OnFireStart -= Fire;
        }

        private void Fire()
        {
            bulletSystem.Fire(
                BulletConfig.Normal,
                firePoint.position,
                firePoint.forward
            );
        }
    }
}


