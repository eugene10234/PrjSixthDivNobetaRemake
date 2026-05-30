using GDCO.Core.Events;

namespace GDCO.Combat.Events
{
    /// <summary>
    /// Buff 應用事件
    /// 使用 struct 避免 GC
    /// </summary>
    public struct BuffAppliedEvent: IGameEvent
    {
        public int TargetEntityId;
        public string BuffId;
        public string BuffType;
    }
}
