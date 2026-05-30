using System;

namespace GDCO.Combat.Entities
{
    /// <summary>
    /// 生命值介面
    /// </summary>
    public interface IHealth
    {
        float Current { get; }
        float Max { get; }
        float Percentage { get; }
        bool IsAlive { get; }
        Action OnDeath { get; set; }

        void Damage(float amount);
        void Heal(float amount);
        void SetMax(float maxHealth);
    }

    /// <summary>
    /// 生命值組件 (純數據 + 邏輯)
    /// 完全 Plain C#
    /// </summary>
    public class Health : IHealth
    {
        public float Current { get; private set; }
        public float Max { get; private set; }
        public float Percentage => Max > 0 ? Current / Max : 0f;
        public bool IsAlive => Current > 0;

        Action IHealth.OnDeath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event Action<float> OnDamaged;   // 實際受傷量
        public event Action<float> OnHealed;
        public event Action OnDeath;

        public Health(float maxHealth)
        {
            Max = maxHealth;
            Current = maxHealth;
        }

        public void Damage(float amount)
        {
            if (amount <= 0 || !IsAlive) return;
            
            float actualDamage = Math.Min(amount, Current);
            Current -= actualDamage;
            
            OnDamaged?.Invoke(actualDamage);
            
            if (Current <= 0)
            {
                Current = 0;
                OnDeath?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0 || !IsAlive) return;
            
            float actualHeal = Math.Min(amount, Max - Current);
            Current += actualHeal;
            OnHealed?.Invoke(actualHeal);
        }

        public void SetMax(float maxHealth)
        {
            float ratio = Max > 0 ? Current / Max : 1f;
            Max = maxHealth;
            Current = Max * ratio;
        }
    }
}
