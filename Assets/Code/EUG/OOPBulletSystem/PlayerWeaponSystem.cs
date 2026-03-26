using Nobeta.EUG.AANEW.MBbulletsystems;
using Nobeta.EUG.BulletSystem.OOP;
using Nobeta.EUG.Events;
//using Nobeta.EUG.Core.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.MBFunctions.PlayerUI;
using Nobeta.EUG.Services;
using System;
using System.Collections.Generic;
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
    public class PlayerWeaponSystem : MonoBehaviour
    {
        [SerializeField] private BulletSystemBridge bulletSystem;
        [SerializeField] private Transform firePoint;

        [Inject] private IPlayerInputService inputService;
        [Inject] private ICameraService cameraService;
        [Inject] private IEventBus _eventBus;

        private void Start()
        {
            //for (int i = 0; i < 1; i++)
            //{
            //    inputService.OnFireEnd += OnFire;
            //}

        }
        private Action<PlayerWeaponFireEvent> _fireAction;
        public Action<PlayerWeaponFireEvent>? InFireAction { get => _fireAction; set => _fireAction = value; }
        private void OnEnable()
        {
            // 訂閱事件 (假設你的 EventBus 是全域單例或透過 DI 注入)
            // 這裡的語法取決於你 EventBus 的具體實作
            Debug.LogWarning("WeaponSystem OnEnable");
            InFireAction = OnNormalFire;
            _eventBus.Subscribe<PlayerWeaponFireEvent>(OnFire);
            _eventBus.Subscribe<PlayerWeaponChangeEvent>(OnChange);
        }

        private void OnDisable()
        {
            // 取消訂閱，防止記憶體洩漏
            _eventBus.Unsubscribe<PlayerWeaponFireEvent>(OnFire);
            _eventBus.Unsubscribe<PlayerWeaponChangeEvent>(OnChange);
        }
        private void OnDestroy()
        {
            //for (int i = 0; i < 1; i++)
            //{
            //    inputService.OnFireEnd -= OnFire;
            //}

        }
        private void OnFire(PlayerWeaponFireEvent evt)
        {
            switch (evt.bulletConfig.Type)
            {   
                case BulletType.Normal:
                    OnNormalFire(evt);
                    break;
                case BulletType.Ice:
                    OnIceFire(evt);
                    break;
                case BulletType.Fire:
                    OnShotgunFire(evt);
                    break;
                case BulletType.Lightning:
                    OnSniperFire(evt);
                    break;
                default:
                    break;
            }
        }
        private void OnChange(PlayerWeaponChangeEvent evt)
        {
            //ChangeBulletFunc(evt.bulletConfig.Type);
        }
        private void ChangeBulletFunc(BulletType btype)
        {
            switch (btype)
            {
                case BulletType.Normal:
                    InFireAction = OnNormalFire;
                    break;
                case BulletType.Ice:
                    InFireAction = OnIceFire;
                    break;
                case BulletType.Fire:
                    InFireAction = OnShotgunFire;
                    break;
                case BulletType.Lightning:
                    InFireAction = OnSniperFire;
                    break;
                default:
                    break;
            }
        }
        private void OnNormalFire(PlayerWeaponFireEvent evt)
        {
            Vector3 targetPoint = GetPlayerTargetPoint();
            bulletSystem.Fire(
                BulletConfig.Normal,
                firePoint.position,
                targetPoint - firePoint.position
            );
        }
        private void OnIceFire(PlayerWeaponFireEvent evt)
        {
            Vector3 targetPoint = GetPlayerTargetPoint();
            bulletSystem.Fire(
                BulletConfig.IceBall,
                firePoint.position,
                targetPoint - firePoint.position
            );
        }
        private void OnShotgunFire(PlayerWeaponFireEvent evt)
        {
            Vector3 targetPoint = GetPlayerTargetPoint();
            bulletSystem.FireCircleShotgunPattern(
                BulletConfig.FireBall,
                firePoint.position,
                targetPoint - firePoint.position, 10, 8f
            );
        }
        private void OnSniperFire(PlayerWeaponFireEvent evt)
        {
            Vector3 targetPoint = GetPlayerTargetPoint();
            bulletSystem.Fire(
                BulletConfig.LightningStrike,
                firePoint.position,
                targetPoint - firePoint.position
            );
        }
        private Vector3 GetPlayerTargetPoint()
        {
            Vector3 targetPoint = Camera.main.transform.position + Camera.main.transform.forward * 75f;
            if (cameraService.TryGetCameraSphereHit(0.2f, Mathf.Infinity, out RaycastHit hitInfo))
            {
                // 1. 取得擊中點
                targetPoint = hitInfo.point;

                // 2. 取得擊中物件的名字
                Debug.Log("擊中了: " + hitInfo.collider.name);

                // 3. 在擊中位置畫一顆小紅球（Debug 用）
                Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red);
            }

            return targetPoint;
        }
        //+ UnityEngine.Random.Range(-20f, 20f) * firePoint.up + UnityEngine.Random.Range(-20f, 20f) * firePoint.right
        //private void OnGUI()
        //{
        //    if (true)
        //    {
        //        DrawDebugInfo();
        //    }

        //}
        private void DrawDebugInfo()
        {

            if (!Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));

            if (GUILayout.Button("Fire"))
            {
                _eventBus.Publish(new PlayerWeaponFireEvent
                {
                    bulletConfig = BulletConfig.FireBall,
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


