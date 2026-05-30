// ============================================================================
// Core Services Implementation - 核心服務實現
// 提供接口的具體實現，展示如何實現各個服務
// ============================================================================

using GDCO.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GDCO.Core.Events
{
    // ========================================================================
    // EventBus - 事件總線實現
    // ========================================================================
    
    /// <summary>
    /// 事件總線 - 全局事件分發系統
    /// 使用字典存儲事件訂閱，支持類型安全的事件發布/訂閱
    /// </summary>
    public class EventBusSimp : IEventBus, IDisposableService
    {
        private readonly Dictionary<Type, List<Delegate>> eventSubscribers = new Dictionary<Type, List<Delegate>>();
        
        public void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            Type eventType = typeof(T);
            
            if (!eventSubscribers.ContainsKey(eventType))
            {
                eventSubscribers[eventType] = new List<Delegate>();
            }
            
            eventSubscribers[eventType].Add(handler);
        }
        
        public void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            Type eventType = typeof(T);
            
            if (eventSubscribers.ContainsKey(eventType))
            {
                eventSubscribers[eventType].Remove(handler);
            }
        }
        
        public void Publish<T>(T gameEvent) where T : IGameEvent
        {
            Type eventType = typeof(T);
            
            if (!eventSubscribers.ContainsKey(eventType)) return;
            
            foreach (var subscriber in eventSubscribers[eventType])
            {
                (subscriber as Action<T>)?.Invoke(gameEvent);
            }
        }
        
        public void Clear()
        {
            eventSubscribers.Clear();
        }
        
        public void Cleanup()
        {
            Clear();
        }
        
        public void Dispose()
        {
            Clear();
        }

        public void Subscribe<T>(Action<T> handler, int priority = 0) where T : IGameEvent
        {
            throw new NotImplementedException();
        }
    }
}
