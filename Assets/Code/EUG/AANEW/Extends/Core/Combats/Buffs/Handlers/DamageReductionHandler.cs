using GDCO.Combat.Damage;
using System;

namespace GDCO.Combat.Buffs.Handlers
{
    /// <summary>
    /// 傷害減免處理器
    /// 按百分比減少受到的傷害
    /// </summary>
    public class DamageReductionHandler : IDamageHandler
    {
        public string Id { get; }
        public int Priority => 30; // 減免在護盾之後、反彈之前

        private readonly float _reductionPercentage;

        public DamageReductionHandler(string id, float reductionPercentage)
        {
            Id = id;
            _reductionPercentage = Math.Clamp(reductionPercentage, 0f, 1f);
        }

        public IDamageContext Handle(IDamageContext context)
        {
            // 減少傷害
            context.Amount *= (1f - _reductionPercentage);
            return context;
        }

        /// <summary>
        /// 取得減免百分比
        /// </summary>
        public float ReductionPercentage => _reductionPercentage;
    }

    /// <summary>
    /// 傷害減免 Buff
    /// </summary>
    public class DamageReductionBuff : IBuff
    {
        public string Id { get; }
        public string Name => "Damage Reduction";
        public float Duration { get; }
        public bool IsPermanent => Duration <= 0;

        private readonly float _reductionPercentage;

        public DamageReductionBuff(float reductionPercentage, float duration = 0f)
        {
            Id = $"reduction_{Guid.NewGuid():N}";
            _reductionPercentage = reductionPercentage;
            Duration = duration;
        }

        public IDamageHandler CreateDamageHandler()
        {
            return new DamageReductionHandler(Id, _reductionPercentage);
        }
    }
}
