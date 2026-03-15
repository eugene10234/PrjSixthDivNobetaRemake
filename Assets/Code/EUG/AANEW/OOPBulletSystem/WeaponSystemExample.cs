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
        [Inject] private ICameraService cameraService;

        private void Start()
        {
            for (int i = 0; i < 1; i++)
            {
                inputService.OnFireStart += Fire;
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < 1; i++)
            {
                inputService.OnFireStart -= Fire;
            }
        }

        private void Fire()
        {
            Vector3 targetPoint = firePoint.position+cameraService.GetCameraForward();
            if (cameraService.TryGetCameraSphereHit(0.2f, 50f, out RaycastHit hitInfo))
            {
                // 1. 取得擊中點
                targetPoint = hitInfo.point;

                // 2. 取得擊中物件的名字
                Debug.Log("擊中了: " + hitInfo.collider.name);

                // 3. 在擊中位置畫一顆小紅球（Debug 用）
                Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red);
            }
            bulletSystem.Fire(
                BulletConfig.Normal,
                firePoint.position ,
                targetPoint- firePoint.position
            );
        }
        //+ UnityEngine.Random.Range(-20f, 20f) * firePoint.up + UnityEngine.Random.Range(-20f, 20f) * firePoint.right
    }
}


