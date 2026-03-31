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
    // Pool<T> - 泛型對象池
    // ========================================================================

    /// <summary>
    /// 泛型對象池
    /// 
    /// 特性：
    /// - 零 GC
    /// - 自動擴展
    /// - 預熱支持
    /// - 統計監控
    /// - 自動清理
    /// </summary>
    public class Pool<T> where T : Component
    {
        private readonly Stack<T> pool;
        private readonly HashSet<T> activeObjects;
        private readonly Transform poolRoot;
        private readonly T prefab;
        private readonly PoolConfig config;

        // 統計數據
        private int totalCreated;
        private int totalGets;
        private int totalReturns;

        // 事件
        public event Action<T> OnObjectCreated;
        public event Action<T> OnObjectSpawned;
        public event Action<T> OnObjectDespawned;
        public event Action OnPoolExhausted;

        #region Constructor

        public Pool(T prefab, PoolConfig config = null)
        {
            this.prefab = prefab;
            this.config = config ?? new PoolConfig();

            pool = new Stack<T>(this.config.InitialCapacity);
            activeObjects = new HashSet<T>();

            // 創建池根節點
            poolRoot = new GameObject($"[Pool] {typeof(T).Name}").transform;
            if (this.config.DontDestroyOnLoad)
            {
                UnityEngine.Object.DontDestroyOnLoad(poolRoot.gameObject);
            }

            // 預熱
            if (this.config.PrewarmCount > 0)
            {
                Prewarm(this.config.PrewarmCount);
            }
        }

        #endregion

        #region Get/Return（獲取/歸還）

        /// <summary>
        /// 從池中獲取對象
        /// </summary>
        public T Get()
        {
            T obj;

            if (pool.Count > 0)
            {
                // 從池中取出
                obj = pool.Pop();
            }
            else
            {
                // 池空了，檢查是否可以創建新對象
                if (config.MaxCapacity > 0 && totalCreated >= config.MaxCapacity)
                {
                    // 達到最大容量
                    OnPoolExhausted?.Invoke();

                    if (config.ThrowOnExhausted)
                    {
                        throw new InvalidOperationException(
                            $"對象池已耗盡: {typeof(T).Name}, 最大容量: {config.MaxCapacity}");
                    }

                    // 強制回收最老的對象（可選）
                    if (config.RecycleOldestOnExhausted)
                    {
                        obj = GetOldestActive();
                        Return(obj);
                        return Get();
                    }

                    return null;
                }

                // 創建新對象
                obj = CreateNewObject();
            }

            // 激活對象
            obj.gameObject.SetActive(true);
            activeObjects.Add(obj);
            totalGets++;

            // 調用生命週期方法
            if (obj is IPoolable poolable)
            {
                poolable.OnSpawn();
            }

            OnObjectSpawned?.Invoke(obj);

            return obj;
        }

        /// <summary>
        /// 獲取多個對象
        /// </summary>
        public List<T> GetMultiple(int count)
        {
            var result = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                var obj = Get();
                if (obj != null)
                {
                    result.Add(obj);
                }
            }
            return result;
        }

        /// <summary>
        /// 歸還對象到池中
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null) return;

            if (!activeObjects.Contains(obj))
            {
                Debug.LogWarning($"嘗試歸還不屬於此池的對象: {obj.name}");
                return;
            }

            // 調用生命週期方法
            if (obj is IPoolable poolable)
            {
                poolable.OnDespawn();
            }

            OnObjectDespawned?.Invoke(obj);

            // 停用對象
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(poolRoot);

            activeObjects.Remove(obj);
            pool.Push(obj);
            totalReturns++;
        }

        /// <summary>
        /// 歸還多個對象
        /// </summary>
        public void ReturnMultiple(IEnumerable<T> objects)
        {
            foreach (var obj in objects)
            {
                Return(obj);
            }
        }

        /// <summary>
        /// 歸還所有活動對象
        /// </summary>
        public void ReturnAll()
        {
            // 複製列表避免迭代時修改
            var objectsToReturn = new List<T>(activeObjects);
            foreach (var obj in objectsToReturn)
            {
                Return(obj);
            }
        }

        #endregion

        #region Prewarm/Capacity（預熱/容量管理）

        /// <summary>
        /// 預熱池（提前創建對象）
        /// </summary>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var obj = CreateNewObject();
                obj.gameObject.SetActive(false);
                pool.Push(obj);
            }
        }

        /// <summary>
        /// 設置容量
        /// </summary>
        public void SetCapacity(int minCapacity, int maxCapacity)
        {
            config.InitialCapacity = minCapacity;
            config.MaxCapacity = maxCapacity;

            // 如果當前池小於最小容量，預熱
            int needToCreate = minCapacity - (pool.Count + activeObjects.Count);
            if (needToCreate > 0)
            {
                Prewarm(needToCreate);
            }
        }

        /// <summary>
        /// 清理未使用的對象
        /// </summary>
        public void Trim()
        {
            while (pool.Count > config.InitialCapacity)
            {
                var obj = pool.Pop();
                UnityEngine.Object.Destroy(obj.gameObject);
                totalCreated--;
            }
        }

        /// <summary>
        /// 縮減到指定大小
        /// </summary>
        public void TrimToSize(int targetSize)
        {
            while (pool.Count > targetSize)
            {
                var obj = pool.Pop();
                UnityEngine.Object.Destroy(obj.gameObject);
                totalCreated--;
            }
        }

        #endregion

        #region Create Object（創建對象）

        private T CreateNewObject()
        {
            var obj = UnityEngine.Object.Instantiate(prefab, poolRoot);
            obj.name = $"{prefab.name}_{totalCreated}";
            obj.gameObject.SetActive(false);

            totalCreated++;

            OnObjectCreated?.Invoke(obj);

            return obj;
        }

        private T GetOldestActive()
        {
            // 簡化實現：返回第一個
            foreach (var obj in activeObjects)
            {
                return obj;
            }
            return null;
        }

        #endregion

        #region Statistics（統計）

        /// <summary>
        /// 獲取池統計信息
        /// </summary>
        public PoolStatistics GetStatistics()
        {
            return new PoolStatistics
            {
                ActiveCount = activeObjects.Count,
                PooledCount = pool.Count,
                TotalCreated = totalCreated,
                TotalGets = totalGets,
                TotalReturns = totalReturns,
                HitRate = totalGets > 0 ? (float)totalReturns / totalGets : 0f
            };
        }

        #endregion

        #region Cleanup（清理）

        /// <summary>
        /// 清除池（銷毀所有對象）
        /// </summary>
        public void Clear()
        {
            // 銷毀所有活動對象
            var objectsToDestroy = new List<T>(activeObjects);
            foreach (var obj in objectsToDestroy)
            {
                if (obj != null)
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }
            activeObjects.Clear();

            // 銷毀池中對象
            while (pool.Count > 0)
            {
                var obj = pool.Pop();
                if (obj != null)
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }

            totalCreated = 0;
            totalGets = 0;
            totalReturns = 0;
        }

        /// <summary>
        /// 銷毀池本身
        /// </summary>
        public void Destroy()
        {
            Clear();
            UnityEngine.Object.Destroy(poolRoot.gameObject);
        }

        #endregion
    }
}