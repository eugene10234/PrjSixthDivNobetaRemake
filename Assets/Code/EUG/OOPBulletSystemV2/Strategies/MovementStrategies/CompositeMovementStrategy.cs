using Nobeta.EUG.BulletSystemV2.OOP;
using System.Linq;
using UnityEngine;
// ── 組合移動（波動 + 追蹤同時）────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public class CompositeMovementStrategy : MovementStrategyBase
    {
        private readonly IMovementStrategy[] _strategies;

        public CompositeMovementStrategy(params IMovementStrategy[] strategies)
        {
            _strategies = strategies;
        }

        public override void OnInitialize(BulletState state)
        {
            foreach (var s in _strategies) s.OnInitialize(state);
        }

        public override void UpdateMovement(BulletState state, float deltaTime)
        {
            foreach (var s in _strategies) s.UpdateMovement(state, deltaTime);
        }

        public override IMovementStrategy Clone()
        {
            var cloned = _strategies.Select(s => s.Clone()).ToArray();
            return new CompositeMovementStrategy(cloned);
        }
    }

}