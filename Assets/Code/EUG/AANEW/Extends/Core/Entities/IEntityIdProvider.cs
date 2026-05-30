namespace GDCO.Core.Entities
{
    /// <summary>
    /// Entity ID 生成器介面
    /// </summary>
    public interface IEntityIdProvider
    {
        int Generate();
    }

    /// <summary>
    /// Entity ID 生成器實現
    /// 線性遞增策略
    /// </summary>
    public class EntityIdProvider : IEntityIdProvider
    {
        private int _nextId = 1;
        
        public int Generate() => _nextId++;
    }
}
