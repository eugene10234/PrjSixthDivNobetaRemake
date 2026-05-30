using GDCO.Core.Entities;
using UnityEngine;

namespace GDCO.Combat.Damage
{
    /// <summary>
    /// 可受傷介面
    /// 符合 ISP (Interface Segregation Principle)
    /// </summary>
    public interface IDamageable:IEntity
    {
        void TakeDamage(float amount, Vector3 position);
    }
}
