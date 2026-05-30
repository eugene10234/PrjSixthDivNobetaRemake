namespace GDCO.Combat.Damage
{
    /// <summary>
    /// 責任鍊節點介面
    /// 每個 Handler 代表一種傷害處理邏輯 (護盾、減免、反彈等)
    /// </summary>
    public interface IDamageHandler
    {
        /// <summary>
        /// Handler 唯一識別符
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// 優先級 (數字越小優先級越高)
        /// 10: 護盾
        /// 30: 減免
        /// 50: 反彈
        /// 100: DOT 等非即時效果
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// 處理傷害上下文
        /// </summary>
        IDamageContext Handle(IDamageContext context);
    }
}
