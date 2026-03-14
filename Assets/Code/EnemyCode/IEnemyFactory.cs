using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.Sam.Enemy
{
    public interface IEnemyFactory<EnemyType>
    {
        EnemyType Create(Vector3 position, Quaternion rotation);
    }
    public class BigGhostFactory : IEnemyFactory<BigGhost>
    {
        private readonly IObjectResolver _container;
        private readonly BigGhost _prefab;

        // VContainer will provide the 'resolver' automatically.
        // The 'prefab' is provided via .WithParameter() in the LifetimeScope.
        public BigGhostFactory(IObjectResolver container, BigGhost prefab)
        {
            _container = container;
            _prefab = prefab;
        }

        public BigGhost Create(Vector3 position, Quaternion rotation)
        {
            return _container.Instantiate(_prefab, position, rotation);
        }
    }
}
