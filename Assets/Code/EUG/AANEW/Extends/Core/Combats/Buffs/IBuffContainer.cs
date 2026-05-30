using System.Collections.Generic;

namespace GDCO.Combat.Buffs
{
    /// <summary>
    /// Buff 容器介面
    /// </summary>
    public interface IBuffContainer
    {
        void Add(IBuff buff);
        void Remove(string buffId);
        bool TryGet(string buffId, out IBuff buff);
        bool Contains(string buffId);
        IReadOnlyList<IBuff> GetAll();
        void Clear();
    }
}
