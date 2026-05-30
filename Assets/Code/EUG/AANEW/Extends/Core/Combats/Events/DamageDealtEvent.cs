using GDCO.Core.Events;
using UnityEngine;

namespace GDCO.Combat.Events
{
    /// <summary>
    /// 傷害造成事件
    /// 使用 struct 避免 GC
    /// </summary>
    public struct DamageDealtEvent:IGameEvent
    {
        public int TargetEntityId;
        public float OriginalAmount;
        public float FinalAmount;
        public Vector3 Position;
    }
}
