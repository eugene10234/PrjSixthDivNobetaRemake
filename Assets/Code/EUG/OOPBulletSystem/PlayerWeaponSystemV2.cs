using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using UnityEngine;
using VContainer;
// ============================================================================
// PlayerWeaponSystem v2  ── 示範如何搭配 Strategy 使用
// ============================================================================
namespace Nobeta.EUG.BulletSystemV2.OOP.UnityBridge
{
    public class PlayerWeaponSystemV2 : MonoBehaviour
    {
        [SerializeField] private BulletSystemBridgeV2 _bulletSystem;
        [SerializeField] private Transform _firePoint;

        [Inject] private IPlayerInputService _inputService;
        [Inject] private ICameraService _cameraService;
        [Inject] private IEventBus _eventBus;

        // ── 定時發射的 handle（取消用）────────────────────────────────────
        private int _intervalFireHandle = -1;

        private void OnEnable()
        {
            _eventBus.Subscribe<PlayerWeaponFireEvent>(OnFire);
            _eventBus.Subscribe<PlayerWeaponChangeEvent>(OnChange);
        }

        private void OnDisable()
        {
            _eventBus.Unsubscribe<PlayerWeaponFireEvent>(OnFire);
            _eventBus.Unsubscribe<PlayerWeaponChangeEvent>(OnChange);

            // 取消任何進行中的定時發射
            if (_intervalFireHandle >= 0)
                _bulletSystem.CancelIntervalFire(_intervalFireHandle);
        }

        // ── Fire 路由（原版邏輯不動）──────────────────────────────────────

        private void OnFire(PlayerWeaponFireEvent evt)
        {
            switch (evt.bulletConfig.Type)
            {
                case BulletType.Normal: OnNormalFire(evt); break;
                case BulletType.Ice: OnIceFire(evt); break;
                case BulletType.Fire: OnFireBallFire(evt); break;
                case BulletType.Lightning: OnSniperFire(evt); break;
            }
        }

        private void OnChange(PlayerWeaponChangeEvent evt) { }

        // ── 各武器實作 ────────────────────────────────────────────────────

        private void OnNormalFire(PlayerWeaponFireEvent evt)
        {
            // 直線單顆（BulletConfig.Normal 已含 SingleSpawn + Forward + Single Damage）
            _bulletSystem.Fire(BulletConfig.Normal, _firePoint.position, GetAimDirection());
        }

        private void OnIceFire(PlayerWeaponFireEvent evt)
        {
            // 冰矢：直線 + 冰凍 DoT
            _bulletSystem.Fire(BulletConfig.IceBall, _firePoint.position, GetAimDirection());
        }

        private void OnFireBallFire(PlayerWeaponFireEvent evt)
        {
            // 火球：BurstSpawn(5顆) + Parabolic + AoeDamage（全在 BulletConfig.FireBall 設定好）
            _bulletSystem.Fire(BulletConfig.FireBall, _firePoint.position, GetAimDirection());
        }

        private void OnSniperFire(PlayerWeaponFireEvent evt)
        {
            // 閃電：單顆追尾導彈
            _bulletSystem.Fire(BulletConfig.LightningStrike, _firePoint.position, GetAimDirection());
        }

        // ── 定時發射範例：每 2 秒自動射出一顆火球 ───────────────────────

        public void StartAutoFireball()
        {
            if (_intervalFireHandle >= 0) return;

            // 建立一個 IntervalSpawnStrategy 的設定
            var intervalConfig = new BulletConfig
            {
                Speed = 15f,
                Damage = 30,
                Lifetime = 4f,
                ColliderRadius = 0.3f,
                Type = BulletType.Fire,
                SpawnStrategy = new IntervalSpawnStrategy(
                    interval: 2f,
                    inner: new BurstSpawnStrategy(count: 1)),   // 每 2 秒 1 顆
                MovementStrategy = new ParabolicMovementStrategy(),
                DamageStrategy = new AoeDamageStrategy(radius: 3f),
            };

            _intervalFireHandle = _bulletSystem.RegisterIntervalFire(
                intervalConfig,
                originGetter: () => _firePoint.position,
                forwardGetter: () => GetAimDirection()
            );
        }

        public void StopAutoFireball()
        {
            if (_intervalFireHandle < 0) return;
            _bulletSystem.CancelIntervalFire(_intervalFireHandle);
            _intervalFireHandle = -1;
        }

        // ── 燃燒彈範例（手動調用）────────────────────────────────────────

        public void FireBurnBomb()
        {
            // 範圍傷害 + 持續燃燒 DoT（BulletConfig.BurnBomb 已配置）
            _bulletSystem.Fire(BulletConfig.BurnBomb, _firePoint.position, GetAimDirection());
        }

        // ── 瞄準方向（原版邏輯不動）──────────────────────────────────────

        private Vector3 GetAimDirection()
        {
            Vector3 target = Camera.main.transform.position + Camera.main.transform.forward * 75f;

            if (_cameraService.TryGetCameraSphereHit(0.2f, Mathf.Infinity, out RaycastHit hit,
                LayerMask.GetMask("Enemy", "Ground", "Obstacle")))
            {
                target = hit.point;
            }

            return (target - _firePoint.position).normalized;
        }
    }
}