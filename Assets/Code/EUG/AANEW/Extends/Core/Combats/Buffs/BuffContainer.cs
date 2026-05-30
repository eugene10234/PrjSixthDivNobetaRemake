using System.Collections.Generic;
using System.Linq;

namespace GDCO.Combat.Buffs
{
    /// <summary>
    /// Buff 容器實現
    /// 使用 Dictionary 提供 O(1) 查找
    /// </summary>
    public class BuffContainer : IBuffContainer
    {
        private readonly Dictionary<string, IBuff> _buffs = new();

        public void Add(IBuff buff)
        {
            _buffs[buff.Id] = buff;
        }

        public void Remove(string buffId)
        {
            _buffs.Remove(buffId);
        }

        public bool TryGet(string buffId, out IBuff buff)
        {
            return _buffs.TryGetValue(buffId, out buff);
        }

        public bool Contains(string buffId)
        {
            return _buffs.ContainsKey(buffId);
        }

        public IReadOnlyList<IBuff> GetAll()
        {
            return _buffs.Values.ToList();
        }

        public void Clear()
        {
            _buffs.Clear();
        }
    }
}
