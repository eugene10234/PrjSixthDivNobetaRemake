
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public abstract class MovementStrategyBase : IMovementStrategy
    {
        // ✅ 所有子類都不需要自己寫 Clone
        public IMovementStrategy Clone()
        {
            return (IMovementStrategy)MemberwiseClone();
        }
        public abstract void OnInitialize(BulletState state);
        public abstract void UpdateMovement(BulletState state, float deltaTime);
    }
}
