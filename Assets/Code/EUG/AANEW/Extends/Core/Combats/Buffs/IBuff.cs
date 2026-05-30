using GDCO.Combat.Damage;

namespace GDCO.Combat.Buffs
{
    /// <summary>
    /// Buff 介面
    /// </summary>
    public interface IBuff
    {
        /// <summary>
        /// Buff 唯一識別符
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// Buff 顯示名稱
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// 持續時間 (秒)
        /// </summary>
        float Duration { get; }
        
        /// <summary>
        /// 是否永久 Buff
        /// </summary>
        bool IsPermanent { get; }
        
        /// <summary>
        /// 創建對應的傷害處理器 (如果有)
        /// 返回 null 表示此 Buff 不影響傷害計算
        /// </summary>
        IDamageHandler CreateDamageHandler();
    }
}
