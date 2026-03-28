using Cysharp.Threading.Tasks;
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
        private  EnemyManager _parentScope;

        [Inject]
        public BigGhostFactory (EnemyManager parentScope)
        {
            _parentScope = parentScope;
        }
        public BigGhost Create(Vector3 position, Quaternion rotation)
        {
            throw new InvalidOperationException("Use Create(position, rotation, prefab) instead.");
        }
        public async UniTask<BigGhost> CreateAsync(Vector3 position, Quaternion rotation, BGLifetimeScope prefab)
        {
            // Instantiation and Injection happen here
            var scopedInstance = _parentScope.CreateChildFromPrefab(prefab);
            var ghost = scopedInstance.Container.Resolve<BigGhost>();

            ghost.transform.SetPositionAndRotation(position, rotation);

            // Optional: Wait one frame if you want to spread the Resolve load
            await UniTask.Yield(); 

            return ghost;
        }
    }

    public class FireDemonFactory: IEnemyFactory<FireDemon> 
    {
        private EnemyManager _parentScope;
        private FireDemonLifeTimeScope _prefabScope;

        [Inject]
        public FireDemonFactory(EnemyManager parentScope, FireDemonLifeTimeScope prefabScope)
        {
            _parentScope = parentScope;
            _prefabScope = prefabScope;
        }
        public FireDemon Create(Vector3 position, Quaternion rotation)
        {
            var scopedInstance = _parentScope.CreateChildFromPrefab(_prefabScope);
            var demon = scopedInstance.Container.Resolve<FireDemon>();
            //var ghost = scopedInstance.GetComponent<BigGhost>();
            //DON'T DO THIS IT'S GONNA SKIP [Inject] Construct Because GetComponent only gets monobehaviour
            demon.transform.SetPositionAndRotation(position, rotation);
            return demon;
        }
    }

    public class GiantBeastGhostFactory : IEnemyFactory<GiantBeastGhost>
    {
        private EnemyManager _parentScope;
        private GBGLifetimeScope _prefabScope;

        [Inject]
        public GiantBeastGhostFactory(EnemyManager parentScope, GBGLifetimeScope prefabScope)
        {
            _parentScope = parentScope;
            _prefabScope = prefabScope;
        }
        public GiantBeastGhost Create(Vector3 position, Quaternion rotation)
        {
            var scopedInstance = _parentScope.CreateChildFromPrefab(_prefabScope);
            var gbg = scopedInstance.Container.Resolve<GiantBeastGhost>();
            //var ghost = scopedInstance.GetComponent<BigGhost>();
            //DON'T DO THIS IT'S GONNA SKIP [Inject] Construct Because GetComponent only gets monobehaviour
            gbg.transform.SetPositionAndRotation(position, rotation);
            return gbg;
        }
    }
    public class MageGhostFactory : IEnemyFactory<MageGhost>
    {
        private EnemyManager _parentScope;
        private MGLifetimeScope _prefabScope;

        [Inject]
        public MageGhostFactory(EnemyManager parentScope, MGLifetimeScope prefabScope)
        {
            _parentScope = parentScope;
            _prefabScope = prefabScope;
        }
        public MageGhost Create(Vector3 position, Quaternion rotation)
        {
            var scopedInstance = _parentScope.CreateChildFromPrefab(_prefabScope);
            var gbg = scopedInstance.Container.Resolve<MageGhost>();
            //var ghost = scopedInstance.GetComponent<BigGhost>();
            //DON'T DO THIS IT'S GONNA SKIP [Inject] Construct Because GetComponent only gets monobehaviour
            gbg.transform.SetPositionAndRotation(position, rotation);
            return gbg;
        }
    }
}
