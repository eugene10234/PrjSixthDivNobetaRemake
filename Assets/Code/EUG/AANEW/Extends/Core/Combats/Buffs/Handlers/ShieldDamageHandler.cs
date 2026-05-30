using GDCO.Combat.Damage;
using System;

namespace GDCO.Combat.Buffs.Handlers
{
    /// <summary>
    /// 護盾 Buff 傷害處理器
    /// 優先級最高，先吸收傷害
    /// </summary>
    public class ShieldDamageHandler : IDamageHandler
    {
        public string Id { get; }
        public int Priority => 10; // 護盾優先處理

        private float _shieldValue;

        public ShieldDamageHandler(string id, float shieldValue)
        {
            Id = id;
            _shieldValue = shieldValue;
        }

        public IDamageContext Handle(IDamageContext context)
        {
            if (_shieldValue > 0)
            {
                float absorbed = Math.Min(context.Amount, _shieldValue);
                _shieldValue -= absorbed;
                context.Amount -= absorbed;

                // 護盾耗盡也不阻擋，讓剩餘傷害繼續
                if (context.Amount <= 0)
                {
                    context.Amount = 0;
                }
            }

            return context;
        }

        /// <summary>
        /// 取得剩餘護盾值
        /// </summary>
        public float RemainingShield => _shieldValue;
    }

    /// <summary>
    /// 護盾 Buff
    /// </summary>
    public class ShieldBuff : IBuff
    {
        public string Id { get; }
        public string Name => "Shield";
        public float Duration { get; }
        public bool IsPermanent => Duration <= 0;

        private readonly float _shieldValue;

        public ShieldBuff(float shieldValue, float duration = 0f)
        {
            Id = $"shield_{Guid.NewGuid():N}";
            _shieldValue = shieldValue;
            Duration = duration;
        }

        public IDamageHandler CreateDamageHandler()
        {
            return new ShieldDamageHandler(Id, _shieldValue);
        }
    }
}
