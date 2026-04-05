
using Nobeta.EUG.Interfaces;
using UnityEngine;

// ============================================================================
// Bullet v2  ── Pure C# 核心子彈類
//
// 變更說明：
//   - 內部狀態改由 BulletState 持有（方便 Strategy 讀寫）
//   - 移動邏輯委託給 IMovementStrategy
//   - 傷害結算委託給 IDamageStrategy
//   - Bullet 自身只負責「更新生命週期 + 呼叫 Strategy + 觸發碰撞」
// ============================================================================
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public class Bullet
    {
        // ── 對外公開 State（Bridge / VisualService 用）────────────────────
        public BulletState State { get; } = new BulletState();

        // 向後相容屬性（BulletManager / Bridge 原有呼叫不需修改）
        public bool IsActive { get => State.IsActive; set => State.IsActive = value; }
        public Vector3 Position { get => State.Position; set => State.Position = value; }
        public Quaternion Rotation { get => State.Rotation; set => State.Rotation = value; }
        public int Id => State.Id;

        // ── Strategy（Initialize 時從 Config 取得）───────────────────────
        private IMovementStrategy _movementStrategy;
        private IDamageStrategy _damageStrategy;

        // ── Dependencies ──────────────────────────────────────────────────
        private readonly IEventBus _eventBus;
        private readonly IPhysicsService _physicsService;

        public Bullet(IEventBus eventBus, IPhysicsService physicsService)
        {
            _eventBus = eventBus;
            _physicsService = physicsService;
        }

        // ── Initialize（從池中取出時呼叫）────────────────────────────────

        public void Initialize(BulletConfig config, Vector3 position, Vector3 direction)
        {
            var s = State;
            s.Id = config.Id;
            s.Position = position;
            s.Direction = direction.normalized;
            s.Speed = config.Speed;
            s.Rotation = Quaternion.LookRotation(direction);
            s.Damage = config.Damage;
            s.Type = config.Type;
            s.Lifetime = config.Lifetime;
            s.ColliderRadius = config.ColliderRadius;
            s.ElapsedTime = 0f;
            s.LaunchPosition = position;
            s.Target = null;
            s.IsActive = true;

            // 取得策略
            _movementStrategy = config.GetMovementStrategy();
            _damageStrategy = config.GetDamageStrategy();

            // 讓 movement strategy 做初始化（設定初速等）
            _movementStrategy.OnInitialize(s);
        }

        // ── Update（每幀由 BulletManager 呼叫）──────────────────────────

        public void Update(float deltaTime)
        {
            if (!State.IsActive) return;

            UpdateLifetime(deltaTime);
            if (!State.IsActive) return;

            // 記住移動前的位置，給 SphereCast 用
            Vector3 prevPosition = State.Position;

            // 移動策略更新位置
            _movementStrategy.Update(State, deltaTime);

            // 碰撞檢查（CCD：掃描從舊位置到新位置）
            CheckCollision(prevPosition);
        }

        // ── Reset（歸還池時呼叫）────────────────────────────────────────

        public void Reset()
        {
            State.Id = 0;
            State.Position = Vector3.zero;
            State.Velocity = Vector3.zero;
            State.Rotation = Quaternion.identity;
            State.IsActive = false;
            State.ElapsedTime = 0f;
            State.Target = null;
            _movementStrategy = null;
            _damageStrategy = null;
        }

        // ── Private ───────────────────────────────────────────────────────

        private void UpdateLifetime(float deltaTime)
        {
            State.Lifetime -= deltaTime;
            if (State.Lifetime <= 0f) OnLifetimeExpired();
        }

        private void CheckCollision(Vector3 prevPosition)
        {
            // 先 OverlapSphere（靜態重疊）
            var hit = _physicsService.OverlapSphere(
                State.Position,
                State.ColliderRadius,
                State.Direction,
                LayerMask.GetMask("Enemy", "Ground", "Obstacle")
            );

            // 再 SphereCast（動態掃描，防高速穿透）
            if (!hit.HasHit)
            {
                // sweepRadius = ColliderRadius + Speed * 補償係數
                // 補償係數確保高速子彈不穿透
                float sweepDist = Vector3.Distance(prevPosition, State.Position) * 1.6f;
                hit = _physicsService.SphereCast(
                    prevPosition,
                    State.ColliderRadius*2,
                    State.Direction,
                    sweepDist,
                    LayerMask.GetMask("Enemy", "Ground", "Obstacle")
                );
            }

            if (hit.HasHit) OnCollision(hit);
        }

        private void OnCollision(CollisionHit hit)
        {
            // 委託給 DamageStrategy（不同子彈有不同結算行為）
            _damageStrategy.ExecuteAsync(State, hit, _eventBus);
            State.IsActive = false;
        }

        private void OnLifetimeExpired()
        {
            _eventBus.Publish(new BulletExpiredEvent
            {
                BulletState = State,
                Position = State.Position
            });
            State.IsActive = false;
        }
    }
}
