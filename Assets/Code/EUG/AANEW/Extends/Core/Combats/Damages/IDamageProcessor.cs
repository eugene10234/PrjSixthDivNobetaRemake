namespace GDCO.Combat.Damage
{
    /// <summary>
    /// 傷害處理器介面 - 管理責任鍊
    /// </summary>
    public interface IDamageProcessor
    {
        DamageResult Process(IDamageContext context);
        void AddHandler(IDamageHandler handler);
        void RemoveHandler(string handlerId);
        void Clear();
    }
}
