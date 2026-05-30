using GDCO.Core.Events;
using UnityEngine;

namespace GDCO.Combat.Events
{
    /// <summary>
    /// 敵人死亡事件
    /// 使用 struct 避免 GC
    /// </summary>
    public struct EnemyDeathEvent: IGameEvent
    {
        public int DeadEnemyId;
        public Vector3 Position;
    }
}
