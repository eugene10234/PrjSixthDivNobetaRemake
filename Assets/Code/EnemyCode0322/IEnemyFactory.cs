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
        private  LifetimeScope _parentScope;
        private  BGLifetimeScope _prefabScope;

        [Inject]
        public void Construct(LifetimeScope parentScope, BGLifetimeScope prefabScope)
        {
            _parentScope = parentScope;
            _prefabScope = prefabScope;
        }
        public BigGhost Create(Vector3 position, Quaternion rotation)
        {
            var scopedInstance = _parentScope.CreateChildFromPrefab(_prefabScope);
            var ghost = scopedInstance.Container.Resolve<BigGhost>();
            //var ghost = scopedInstance.GetComponent<BigGhost>();
            ghost.transform.SetPositionAndRotation(position, rotation);
            return ghost;
        }
    }
}
