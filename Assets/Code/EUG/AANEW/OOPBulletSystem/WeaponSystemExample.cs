using System.Collections.Generic;
using Nobeta.EUG.AANEW.MBbulletsystems;
using Nobeta.EUG.BulletSystem.OOP;
//using Nobeta.EUG.Core.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.MBFunctions.PlayerUI;
using Nobeta.EUG.Services;
using Unity.Mathematics;
using Unity.VisualScripting;
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
        [Inject] private  IEventBus _eventBus;

        private void Start()
        {
            //for (int i = 0; i < 1; i++)
            //{
            //    inputService.OnFireEnd += OnFire;
            //}
            
        }
        private void OnEnable()
        {
            // 訂閱事件 (假設你的 EventBus 是全域單例或透過 DI 注入)
            // 這裡的語法取決於你 EventBus 的具體實作
            Debug.LogWarning("WeaponSystem OnEnable");
            _eventBus.Subscribe<WeaponFiredEvent2>(OnFire);
        }

        private void OnDisable()
        {
            // 取消訂閱，防止記憶體洩漏
            _eventBus.Unsubscribe<WeaponFiredEvent2>(OnFire);
        }
        private void OnDestroy()
        {
            //for (int i = 0; i < 1; i++)
            //{
            //    inputService.OnFireEnd -= OnFire;
            //}
           
        }

        private void OnFire(WeaponFiredEvent2 evt)
        {
            Vector3 targetPoint = firePoint.position+ Camera.main.transform.forward*50f;
            if (cameraService.TryGetCameraSphereHit(0.2f, Mathf.Infinity, out RaycastHit hitInfo))
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
        private void OnGUI()
        {
            if (true)
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
                _eventBus.Publish(new WeaponFiredEvent2
                {
  
                });
            }
            //if (GUILayout.Button("SwitchToFireBall"))
            //{
            //    SwitchToFireBall();
            //}
            //if (GUILayout.Button("SwitchToIceBall"))
            //{
            //    SwitchToIceBall();
            //}
            //if (GUILayout.Button("SwitchToLightningBall"))
            //{
            //    SwitchToLightningBall();
            //}
            GUILayout.EndArea();

        }
    }
}


