// ============================================================================
// Professional ObjectPool - 專業級對象池
// 零 GC、高性能、功能完整的對象池系統
// ============================================================================

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Nobeta.EUG.Core.Pooling
{
    // ========================================================================
    // PoolManager - 池管理器
    // ========================================================================

    /// <summary>
    /// 對象池管理器
    /// 統一管理所有對象池
    /// </summary>
    public class PoolManager
    {
        private readonly Dictionary<Type, object> pools = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> scenePools = new Dictionary<Type, object>();

        /// <summary>
        /// 獲取或創建池
        /// </summary>
        public Pool<T> GetPool<T>(T prefab, PoolConfig config = null) where T : Component
        {
            Type type = typeof(T);

            if (!pools.ContainsKey(type))
            {
                pools[type] = new Pool<T>(prefab, config);
            }

            return pools[type] as Pool<T>;
        }

        /// <summary>
        /// 獲取場景特定的池
        /// </summary>
        public Pool<T> GetScenePool<T>(T prefab, PoolConfig config = null) where T : Component
        {
            Type type = typeof(T);

            if (!scenePools.ContainsKey(type))
            {
                var poolConfig = config ?? new PoolConfig
                {
                    DontDestroyOnLoad = false
                };
                scenePools[type] = new Pool<T>(prefab, poolConfig);
            }

            return scenePools[type] as Pool<T>;
        }

        /// <summary>
        /// 清理所有場景池
        /// </summary>
        public void ClearScenePools()
        {
            foreach (var pool in scenePools.Values)
            {
                var clearMethod = pool.GetType().GetMethod("Clear");
                clearMethod?.Invoke(pool, null);
            }
            scenePools.Clear();
        }

        /// <summary>
        /// 清理所有池
        /// </summary>
        public void ClearAll()
        {
            foreach (var pool in pools.Values)
            {
                var clearMethod = pool.GetType().GetMethod("Clear");
                clearMethod?.Invoke(pool, null);
            }
            pools.Clear();

            ClearScenePools();
        }

        /// <summary>
        /// 獲取所有池的統計信息
        /// </summary>
        public Dictionary<string, PoolStatistics> GetAllStatistics()
        {
            var result = new Dictionary<string, PoolStatistics>();

            foreach (var kvp in pools)
            {
                var statsMethod = kvp.Value.GetType().GetMethod("GetStatistics");
                var stats = statsMethod?.Invoke(kvp.Value, null) as PoolStatistics?;

                if (stats.HasValue)
                {
                    result[kvp.Key.Name] = stats.Value;
                }
            }

            return result;
        }
    }

  
}