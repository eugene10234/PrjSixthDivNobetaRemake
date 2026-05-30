using System.Collections.Generic;
using UnityEngine;

namespace GDCO.Combat.Integration
{
    /// <summary>
    /// 優化碰撞檢測的快速查找
    /// GameObject InstanceID -> Entity ID 映射
    /// 方案 2 的效能優化：減少一次 GetComponent 查找
    /// </summary>
    public interface IEnemyCollisionRegistry
    {
        void Register(GameObject go, int entityId);
        void Unregister(GameObject go);
        bool TryGetEntityId(GameObject go, out int entityId);
    }

    /// <summary>
    /// Collision Registry 實現
    /// Hybrid 方案的 Hot Path 優化
    /// </summary>
    public class EnemyCollisionRegistry : IEnemyCollisionRegistry
    {
        private readonly Dictionary<int, int> _goIdToEntityId = new();

        public void Register(GameObject go, int entityId)
        {
            _goIdToEntityId[go.GetInstanceID()] = entityId;
        }

        public void Unregister(GameObject go)
        {
            _goIdToEntityId.Remove(go.GetInstanceID());
        }

        public bool TryGetEntityId(GameObject go, out int entityId)
        {
            return _goIdToEntityId.TryGetValue(go.GetInstanceID(), out entityId);
        }
    }
}
