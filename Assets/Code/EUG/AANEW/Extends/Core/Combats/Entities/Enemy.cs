using GDCO.Core.Entities;
using GDCO.Combat.Damage;
using GDCO.Combat.Buffs;
using GDCO.Combat.Events;
using UnityEngine;
using GDCO.Core.Events;

namespace GDCO.Combat.Entities
{
    /// <summary>
    /// Enemy 實體 - 完全 Plain C#
    /// 組合各種能力而非繼承
    /// 符合 SOLID 原則
    /// </summary>
    public class Enemy : IEnemy
    {
        public int EntityId { get; }
        public IHealth Health { get; }
        public bool IsAlive => Health.IsAlive;

        private readonly IEntityRegistry _registry;
        private readonly IEventBus _eventBus;
        private readonly IDamageProcessor _damageProcessor;
        private readonly IBuffContainer _buffContainer;

        public Enemy(
            int entityId,
            float maxHealth,
            IEntityRegistry registry,
            IEventBus eventBus,
            IDamageProcessor damageProcessor,
            IBuffContainer buffContainer)
        {
            EntityId = entityId;
            Health = new Health(maxHealth);
            _registry = registry;
            _eventBus = eventBus;
            _damageProcessor = damageProcessor;
            _buffContainer = buffContainer;

            // 訂閱生命值事件
            Health.OnDeath += OnDeath;
        }

        public void Initialize()
        {
            // 註冊到 Entity Registry
            _registry.Register(EntityId, this);
        }

        public void Dispose()
        {
            // 清理註冊
            _registry.Unregister(EntityId);
            
            // 清理事件
            Health.OnDeath -= OnDeath;
            
            // 清理 Buff
            _buffContainer.Clear();
        }

        public void TakeDamage(float amount, Vector3 position)
        {
            if (!IsAlive) return;

            // 責任鍊處理傷害
            var context = new DamageContext(this, amount, position);
            var result = _damageProcessor.Process(context);

            // 應用最終傷害
            if (!result.IsBlocked && result.FinalAmount > 0)
            {
                Health.Damage(result.FinalAmount);
                
                // 發布傷害事件
                _eventBus.Publish(new DamageDealtEvent
                {
                    TargetEntityId = EntityId,
                    OriginalAmount = amount,
                    FinalAmount = result.FinalAmount,
                    Position = position
                });
            }
        }

        public void AddBuff(IBuff buff)
        {
            _buffContainer.Add(buff);
            
            // Buff 可能修改責任鍊
            if (buff.CreateDamageHandler() is IDamageHandler handler)
            {
                _damageProcessor.AddHandler(handler);
            }

            // 發布 Buff 事件
            _eventBus.Publish(new BuffAppliedEvent
            {
                TargetEntityId = EntityId,
                BuffId = buff.Id,
                BuffType = buff.GetType().Name
            });
        }

        public void RemoveBuff(string buffId)
        {
            if (_buffContainer.TryGet(buffId, out var buff))
            {
                _buffContainer.Remove(buffId);
                
                // 移除對應的傷害處理器
                if (buff.CreateDamageHandler() is IDamageHandler handler)
                {
                    _damageProcessor.RemoveHandler(handler.Id);
                }
            }
        }

        public bool HasBuff(string buffId) => _buffContainer.Contains(buffId);

        private void OnDeath()
        {
            // 發布死亡事件
            _eventBus.Publish(new EnemyDeathEvent
            {
                DeadEnemyId = EntityId,
                Position = Vector3.zero // 需要從外部傳入或存儲
            });
        }
    }
}
