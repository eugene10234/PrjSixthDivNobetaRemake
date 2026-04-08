using Nobeta.EUG.BulletSystemV2.OOP;
using System.Linq;
using UnityEngine;
// ── 閃電折射感（ZigZag）───────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public class ZigZagMovementStrategy : MovementStrategyBase
    {
        private readonly float _angle;
        private readonly float _interval;
        private float _timer;
        private int _side = 1;

        public ZigZagMovementStrategy(float angle, float interval)
        {
            _angle = angle;
            _interval = interval;
        }

        public override void OnInitialize(BulletState state)
        {
            _timer = 0f;
            _side = 1;
        }

        public override void UpdateMovement(BulletState state, float deltaTime)
        {
            _timer += deltaTime;
            if (_timer >= _interval)
            {
                _side *= -1;
                state.Velocity = Quaternion.Euler(0, _angle * _side, 0) * state.Velocity;
                _timer = 0f;
            }
        }
    }
}