using GDCO.Core.Entities;
using GDCO.Combat.Damage;
using GDCO.Combat.Buffs;

namespace GDCO.Combat.Entities
{
    /// <summary>
    /// 敵人實體介面
    /// 組合多個能力介面 (符合 ISP)
    /// </summary>
    public interface IEnemy : IEntity, IDamageable, IBuffable
    {
        IHealth Health { get; }
        bool IsAlive { get; }
        
        void Initialize();
        void Dispose();
    }
}
