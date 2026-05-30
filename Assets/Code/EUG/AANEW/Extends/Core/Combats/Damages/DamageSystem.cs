using GDCO.Core.Entities;
using UnityEngine;

namespace GDCO.Combat.Damage
{
    /// <summary>
    /// 傷害系統服務介面 - 提供高層 API
    /// </summary>
    public interface IDamageSystem
    {
        void DealDamage(int targetEntityId, float amount, Vector3 position);
        void DealAreaDamage(Vector3 center, float radius, float amount);
    }

    /// <summary>
    /// 傷害系統實現
    /// </summary>
    public class DamageSystem : IDamageSystem
    {
        private readonly IEntityRegistry _registry;

        public DamageSystem(IEntityRegistry registry)
        {
            _registry = registry;
        }

        public void DealDamage(int targetEntityId, float amount, Vector3 position)
        {
            if (_registry.TryGet<IDamageable>(targetEntityId, out var target))
            {
                target.TakeDamage(amount, position);
            }
        }

        public void DealAreaDamage(Vector3 center, float radius, float amount)
        {
            // 範圍傷害實現
            // 需要配合空間查詢系統 (如 SpatialHash)
            // 簡化版本：遍歷所有實體
            
            // TODO: 實際應用需要優化空間查詢
        }
    }
}
