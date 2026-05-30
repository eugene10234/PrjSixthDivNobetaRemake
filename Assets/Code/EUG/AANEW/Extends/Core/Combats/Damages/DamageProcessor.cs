using System.Collections.Generic;
using System.Linq;

namespace GDCO.Combat.Damage
{
    /// <summary>
    /// 傷害處理器實現 - 責任鍊模式核心
    /// 按優先級執行所有 Handler
    /// </summary>
    public class DamageProcessor : IDamageProcessor
    {
        private readonly List<IDamageHandler> _handlers = new();

        public DamageResult Process(IDamageContext context)
        {
            // 按優先級排序處理
            var sortedHandlers = _handlers.OrderBy(h => h.Priority);
            
            foreach (var handler in sortedHandlers)
            {
                context = handler.Handle(context);
                
                // 如果被完全阻擋，提前結束
                if (context.IsBlocked)
                {
                    return new DamageResult(0, true);
                }
            }

            return new DamageResult(context.Amount, context.IsBlocked);
        }

        public void AddHandler(IDamageHandler handler)
        {
            // 避免重複添加
            if (!_handlers.Any(h => h.Id == handler.Id))
            {
                _handlers.Add(handler);
            }
        }

        public void RemoveHandler(string handlerId)
        {
            _handlers.RemoveAll(h => h.Id == handlerId);
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}
