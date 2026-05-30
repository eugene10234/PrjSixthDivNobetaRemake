using UnityEngine;

namespace GDCO.Combat.Integration
{
    /// <summary>
    /// Unity GameObject 到 Entity ID 的橋接
    /// 唯一的 MonoBehaviour 組件 - 極度輕量
    /// 方案 1 的核心：掛在 GameObject 上提供 Entity ID
    /// </summary>
    public sealed class EntityBridge : MonoBehaviour
    {
        private int _entityId;
        
        /// <summary>
        /// 設置 Entity ID
        /// </summary>
        public void Setup(int entityId)
        {
            _entityId = entityId;
        }
        
        /// <summary>
        /// 取得 Entity ID
        /// </summary>
        public int EntityId => _entityId;
    }
}
