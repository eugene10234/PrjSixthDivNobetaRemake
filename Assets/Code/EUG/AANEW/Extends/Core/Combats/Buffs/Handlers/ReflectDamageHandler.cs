using GDCO.Combat.Damage;
using System;

namespace GDCO.Combat.Buffs.Handlers
{
    /// <summary>
    /// 反彈傷害處理器
    /// 反彈部分傷害回攻擊者
    /// </summary>
    public class ReflectDamageHandler : IDamageHandler
    {
        public string Id { get; }
        public int Priority => 50; // 反彈在護盾和減免之後

        private readonly float _reflectPercentage;

        public ReflectDamageHandler(string id, float reflectPercentage)
        {
            Id = id;
            _reflectPercentage = Math.Clamp(reflectPercentage, 0f, 1f);
        }

        public IDamageContext Handle(IDamageContext context)
        {
            // 計算反彈傷害
            float reflectedAmount = context.Amount * _reflectPercentage;
            
            // TODO: 實際應用需要知道攻擊者並對其造成傷害
            // 需要在 DamageContext 中加入 Attacker 資訊
            // if (context.Attacker is IDamageable attacker)
            // {
            //     attacker.TakeDamage(reflectedAmount, context.Position);
            // }

            // 反彈不影響自身受到的傷害
            return context;
        }

        /// <summary>
        /// 取得反彈百分比
        /// </summary>
        public float ReflectPercentage => _reflectPercentage;
    }

    /// <summary>
    /// 反彈傷害 Buff
    /// </summary>
    public class ReflectDamageBuff : IBuff
    {
        public string Id { get; }
        public string Name => "Reflect";
        public float Duration { get; }
        public bool IsPermanent => Duration <= 0;

        private readonly float _reflectPercentage;

        public ReflectDamageBuff(float reflectPercentage, float duration = 0f)
        {
            Id = $"reflect_{Guid.NewGuid():N}";
            _reflectPercentage = reflectPercentage;
            Duration = duration;
        }

        public IDamageHandler CreateDamageHandler()
        {
            return new ReflectDamageHandler(Id, _reflectPercentage);
        }
    }
}
