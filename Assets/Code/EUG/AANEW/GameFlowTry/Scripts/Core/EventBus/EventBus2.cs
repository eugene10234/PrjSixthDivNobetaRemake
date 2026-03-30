using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nobeta.EUG.GameFlowTry.Core.EventBus
{
    public sealed class EventBus2 : IEventBus2
    {
        private readonly Dictionary<Type, Delegate> _listeners = new();

        public void Subscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            _listeners[type] = _listeners.TryGetValue(type, out var existing)
                ? (Action<T>)existing + listener
                : listener;
        }

        public void Unsubscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (_listeners.TryGetValue(type, out var existing))
                _listeners[type] = (Action<T>)existing - listener;
        }

        public void Publish<T>(T @event)
        {
            if (_listeners.TryGetValue(typeof(T), out var del))
                ((Action<T>)del)?.Invoke(@event);
        }
    }
}
