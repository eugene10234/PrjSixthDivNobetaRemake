using UnityEngine;

namespace GDCO.Combat.Damage
{
    /// <summary>
    /// 傷害上下文介面 - 在責任鍊中傳遞
    /// </summary>
    public interface IDamageContext
    {
        IDamageable Target { get; }
        float OriginalAmount { get; }
        float Amount { get; set; }
        Vector3 Position { get; }
        bool IsBlocked { get; set; }
    }

    /// <summary>
    /// 傷害上下文實現
    /// </summary>
    public class DamageContext : IDamageContext
    {
        public IDamageable Target { get; }
        public float OriginalAmount { get; }
        public float Amount { get; set; }
        public Vector3 Position { get; }
        public bool IsBlocked { get; set; }

        public DamageContext(IDamageable target, float amount, Vector3 position)
        {
            Target = target;
            OriginalAmount = amount;
            Amount = amount;
            Position = position;
            IsBlocked = false;
        }
    }

    /// <summary>
    /// 傷害處理結果 (struct 避免 GC)
    /// </summary>
    public struct DamageResult
    {
        public float FinalAmount;
        public bool IsBlocked;

        public DamageResult(float finalAmount, bool isBlocked)
        {
            FinalAmount = finalAmount;
            IsBlocked = isBlocked;
        }
    }
}
