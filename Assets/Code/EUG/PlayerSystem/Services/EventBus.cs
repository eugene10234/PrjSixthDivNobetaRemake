using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// ========================================================================
// EventBus - 事件總線
// ========================================================================

namespace Nobeta.EUG.FSM.Services
{
    // ============================================================================
    // Professional EventBus - 專業級事件總線
    // 高性能、類型安全、功能完整的事件系統
    // ============================================================================


    // ========================================================================
    // EventBus - 主要實現
    // ========================================================================

    /// <summary>
    /// 專業級事件總線
    /// 
    /// 特性：
    /// - 類型安全
    /// - 優先級支持
    /// - 事件過濾
    /// - 條件訂閱
    /// - 生命週期管理
    /// - 事件歷史記錄
    /// </summary>
    public class EventBus : IEventBus, IDisposable
    {
        // 訂閱者存儲：事件類型 → 訂閱者列表
        private readonly Dictionary<Type, List<SubscriberInfo>> subscribers
            = new Dictionary<Type, List<SubscriberInfo>>();

        // 延遲事件隊列
        private readonly Queue<object> deferredEvents = new Queue<object>();

        // 事件歷史（用於除錯）
        private readonly Dictionary<Type, Queue<object>> eventHistory
            = new Dictionary<Type, Queue<object>>();

        // 全局過濾器
        private readonly List<Func<object, bool>> globalFilters
            = new List<Func<object, bool>>();

        // 配置
        private readonly EventBusConfig config;

        // 線程鎖
        private readonly object subscriberLock = new object();

        // 統計數據
        private readonly Dictionary<Type, int> eventCounts
            = new Dictionary<Type, int>();

        #region Constructor

        public EventBus(EventBusConfig config = null)
        {
            this.config = config ?? new EventBusConfig();

            if (this.config.EnableLogging)
            {
                Debug.Log("[EventBus] 初始化完成");
            }
        }

        #endregion

        #region Subscribe（訂閱）

        /// <summary>
        /// 訂閱事件
        /// </summary>
        public void Subscribe<T>(Action<T> handler, int priority = 0) where T : struct
        {
            if (handler == null)
            {
                Debug.LogError("[EventBus] Handler 不能為 null");
                return;
            }

            Type eventType = typeof(T);

            lock (subscriberLock)
            {
                if (!subscribers.ContainsKey(eventType))
                {
                    subscribers[eventType] = new List<SubscriberInfo>();
                }

                var subscriberInfo = new SubscriberInfo
                {
                    Handler = handler,
                    Priority = priority,
                    Owner = null
                };

                subscribers[eventType].Add(subscriberInfo);

                // 按優先級排序（高優先級在前）
                subscribers[eventType].Sort((a, b) => b.Priority.CompareTo(a.Priority));
            }

            if (config.EnableLogging)
            {
                Debug.Log($"[EventBus] 訂閱事件: {eventType.Name}, 優先級: {priority}");
            }
        }
        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            Subscribe<T>(handler, 0);
        }
        /// <summary>
        /// 條件訂閱（只處理符合條件的事件）
        /// </summary>
        public void SubscribeConditional<T>(
            Action<T> handler,
            Func<T, bool> condition,
            int priority = 0) where T : struct
        {
            if (handler == null || condition == null)
            {
                Debug.LogError("[EventBus] Handler 或 Condition 不能為 null");
                return;
            }

            // 包裝處理器
            Action<T> wrappedHandler = (evt) =>
            {
                if (condition(evt))
                {
                    handler(evt);
                }
            };

            Subscribe(wrappedHandler, priority);
        }

        /// <summary>
        /// 一次性訂閱（處理一次後自動取消）
        /// </summary>
        public void SubscribeOnce<T>(Action<T> handler, int priority = 0) where T : struct
        {
            Action<T> onceHandler = null;
            onceHandler = (evt) =>
            {
                handler(evt);
                Unsubscribe(onceHandler);
            };

            Subscribe(onceHandler, priority);
        }

        /// <summary>
        /// 帶擁有者的訂閱（方便批量取消）
        /// </summary>
        public void Subscribe<T>(Action<T> handler, object owner, int priority = 0) where T : struct
        {
            if (handler == null || owner == null)
            {
                Debug.LogError("[EventBus] Handler 或 Owner 不能為 null");
                return;
            }

            Type eventType = typeof(T);

            lock (subscriberLock)
            {
                if (!subscribers.ContainsKey(eventType))
                {
                    subscribers[eventType] = new List<SubscriberInfo>();
                }

                var subscriberInfo = new SubscriberInfo
                {
                    Handler = handler,
                    Priority = priority,
                    Owner = owner
                };

                subscribers[eventType].Add(subscriberInfo);
                subscribers[eventType].Sort((a, b) => b.Priority.CompareTo(a.Priority));
            }
        }

        #endregion

        #region Unsubscribe（取消訂閱）

        /// <summary>
        /// 取消訂閱
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) return;

            Type eventType = typeof(T);

            lock (subscriberLock)
            {
                if (subscribers.TryGetValue(eventType, out var subscriberList))
                {
                    subscriberList.RemoveAll(s =>
                        s.Handler is Action<T> action && action == handler);
                }
            }

            if (config.EnableLogging)
            {
                Debug.Log($"[EventBus] 取消訂閱: {eventType.Name}");
            }
        }

        /// <summary>
        /// 取消指定擁有者的所有訂閱
        /// </summary>
        public void UnsubscribeAll(object owner)
        {
            if (owner == null) return;

            lock (subscriberLock)
            {
                foreach (var kvp in subscribers)
                {
                    kvp.Value.RemoveAll(s => s.Owner == owner);
                }
            }

            if (config.EnableLogging)
            {
                Debug.Log($"[EventBus] 取消擁有者的所有訂閱: {owner.GetType().Name}");
            }
        }

        #endregion

        #region Publish（發布）

        /// <summary>
        /// 立即發布事件
        /// </summary>
        public void Publish<T>(T evt) where T : struct
        {
            Type eventType = typeof(T);

            // 全局過濾
            if (!PassGlobalFilters(evt))
            {
                return;
            }

            // 記錄事件
            RecordEvent(evt);

            // 統計
            IncrementEventCount(eventType);

            // 獲取訂閱者列表（複製以避免迭代時修改）
            List<SubscriberInfo> subscribersCopy;
            lock (subscriberLock)
            {
                if (!subscribers.TryGetValue(eventType, out var subscriberList))
                {
                    return; // 沒有訂閱者
                }

                subscribersCopy = new List<SubscriberInfo>(subscriberList);
            }

            // 分發事件
            foreach (var subscriberInfo in subscribersCopy)
            {
                try
                {
                    if (subscriberInfo.Handler is Action<T> typedHandler)
                    {
                        typedHandler(evt);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventBus] 事件處理異常: {eventType.Name}\n{e}");
                }
            }

            if (config.EnableLogging)
            {
                Debug.Log($"[EventBus] 發布事件: {eventType.Name}, 訂閱者: {subscribersCopy.Count}");
            }
        }

        /// <summary>
        /// 延遲發布（在 ProcessDeferredEvents 時處理）
        /// </summary>
        public void PublishDeferred<T>(T evt) where T : struct
        {
            lock (deferredEvents)
            {
                deferredEvents.Enqueue(evt);
            }
        }

        /// <summary>
        /// 處理所有延遲事件
        /// </summary>
        public void ProcessDeferredEvents()
        {
            lock (deferredEvents)
            {
                while (deferredEvents.Count > 0)
                {
                    var evt = deferredEvents.Dequeue();

                    // 使用反射調用 Publish（性能較低，但簡單）
                    var publishMethod = GetType()
                        .GetMethod(nameof(Publish))
                        .MakeGenericMethod(evt.GetType());

                    publishMethod.Invoke(this, new[] { evt });
                }
            }
        }

        #endregion

        #region Filters（過濾器）

        /// <summary>
        /// 添加全局過濾器
        /// </summary>
        public void AddGlobalFilter(Func<object, bool> filter)
        {
            if (filter != null)
            {
                globalFilters.Add(filter);
            }
        }

        /// <summary>
        /// 移除全局過濾器
        /// </summary>
        public void RemoveGlobalFilter(Func<object, bool> filter)
        {
            globalFilters.Remove(filter);
        }

        private bool PassGlobalFilters(object evt)
        {
            foreach (var filter in globalFilters)
            {
                if (!filter(evt))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region History（歷史記錄）

        private void RecordEvent<T>(T evt)
        {
            if (!config.EnableHistory) return;

            Type eventType = typeof(T);

            if (!eventHistory.ContainsKey(eventType))
            {
                eventHistory[eventType] = new Queue<object>();
            }

            var queue = eventHistory[eventType];
            queue.Enqueue(evt);

            // 限制歷史記錄數量
            while (queue.Count > config.MaxHistoryPerEvent)
            {
                queue.Dequeue();
            }
        }

        /// <summary>
        /// 獲取最近的事件歷史
        /// </summary>
        public List<T> GetRecentEvents<T>(int count = 10) where T : struct
        {
            Type eventType = typeof(T);

            if (!eventHistory.TryGetValue(eventType, out var queue))
            {
                return new List<T>();
            }

            return queue
                .Cast<T>()
                .TakeLast(count)
                .ToList();
        }

        /// <summary>
        /// 清除事件歷史
        /// </summary>
        public void ClearHistory()
        {
            eventHistory.Clear();
        }

        #endregion

        #region Statistics（統計）

        private void IncrementEventCount(Type eventType)
        {
            if (!eventCounts.ContainsKey(eventType))
            {
                eventCounts[eventType] = 0;
            }
            eventCounts[eventType]++;
        }

        /// <summary>
        /// 獲取事件統計
        /// </summary>
        public Dictionary<string, int> GetEventStatistics()
        {
            return eventCounts.ToDictionary(
                kvp => kvp.Key.Name,
                kvp => kvp.Value
            );
        }

        /// <summary>
        /// 重置統計
        /// </summary>
        public void ResetStatistics()
        {
            eventCounts.Clear();
        }

        #endregion

        #region Cleanup（清理）

        /// <summary>
        /// 清除所有訂閱
        /// </summary>
        public void Clear()
        {
            lock (subscriberLock)
            {
                subscribers.Clear();
            }

            deferredEvents.Clear();
            eventHistory.Clear();
            eventCounts.Clear();
            globalFilters.Clear();

            if (config.EnableLogging)
            {
                Debug.Log("[EventBus] 已清除所有訂閱和數據");
            }
        }

        public void Dispose()
        {
            Clear();
        }



        #endregion
    }

    // ========================================================================
    // Supporting Classes（輔助類）
    // ========================================================================

    /// <summary>
    /// 訂閱者信息
    /// </summary>
    internal class SubscriberInfo
    {
        public Delegate Handler;
        public int Priority;
        public object Owner;
    }

    /// <summary>
    /// EventBus 配置
    /// </summary>
    public class EventBusConfig
    {
        /// <summary>
        /// 是否啟用日誌
        /// </summary>
        public bool EnableLogging { get; set; } = false;

        /// <summary>
        /// 是否啟用事件歷史記錄
        /// </summary>
        public bool EnableHistory { get; set; } = true;

        /// <summary>
        /// 每個事件類型的最大歷史記錄數
        /// </summary>
        public int MaxHistoryPerEvent { get; set; } = 50;
    }
}

