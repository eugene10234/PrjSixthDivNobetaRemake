using System.Collections.Generic;

namespace GDCO.Core.Entities
{
    /// <summary>
    /// Entity ID 到實體的映射表
    /// 支援多介面查詢，實現方案 1 的核心
    /// </summary>
    public interface IEntityRegistry
    {
        void Register(int entityId, IEntity entity);
        void Unregister(int entityId);
        bool TryGet<T>(int entityId, out T entity) where T : class, IEntity;
    }

    /// <summary>
    /// Entity Registry 實現
    /// 使用 Dictionary 提供 O(1) 查找
    /// </summary>
    public class EntityRegistry : IEntityRegistry
    {
        private readonly Dictionary<int, IEntity> _entities = new();

        public void Register(int entityId, IEntity entity)
        {
            _entities[entityId] = entity;
        }

        public void Unregister(int entityId)
        {
            _entities.Remove(entityId);
        }

        public bool TryGet<T>(int entityId, out T entity) where T : class, IEntity
        {
            if (_entities.TryGetValue(entityId, out var e) && e is T typed)
            {
                entity = typed;
                return true;
            }
            entity = null;
            return false;
        }
    }
}
