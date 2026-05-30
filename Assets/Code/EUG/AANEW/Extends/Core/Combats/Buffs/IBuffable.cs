namespace GDCO.Combat.Buffs
{
    /// <summary>
    /// 可加 Buff 介面
    /// 符合 ISP (Interface Segregation Principle)
    /// </summary>
    public interface IBuffable
    {
        void AddBuff(IBuff buff);
        void RemoveBuff(string buffId);
        bool HasBuff(string buffId);
    }

}
