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
    public class SmallGhostFactory : IEnemyFactory<SmallGhost>
    {
        private EnemyManager _parentScope;
        private SGLifetimeScope _prefabScope;
        [Inject]
        public SmallGhostFactory(EnemyManager parentScope, SGLifetimeScope prefabScope)
        {
            _parentScope = parentScope;
            _prefabScope = prefabScope;
        }
        public SmallGhost Create(Vector3 position, Quaternion rotation)
        {
            if (_prefabScope == null)
                throw new InvalidOperationException(
                    "[SmallGhostFactory] _prefabScope is null. " +
                    "Check that SGLifetimeScope prefab is assigned in EnemyManager inspector.");
            bool wasActive = _prefabScope.gameObject.activeSelf;
            if (!wasActive) _prefabScope.gameObject.SetActive(true);
            var scopedInstance = _parentScope.CreateChildFromPrefab(_prefabScope);
            try
            {
                var sg = scopedInstance.Container.Resolve<SmallGhost>();
                sg.transform.SetPositionAndRotation(position, rotation);
                return sg;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to resolve SmallGhost: {e}");
                throw;
            }
        }
    }
    public class GhostKingFactory : IEnemyFactory<GhostKing>
    {
        private EnemyManager _parentScope;
        private GKLifetimeScope _prefabScope;

        [Inject]
        public GhostKingFactory(EnemyManager parentScope, GKLifetimeScope prefabScope)
        {
            _parentScope = parentScope;
            _prefabScope = prefabScope;
        }

        public GhostKing Create(Vector3 position, Quaternion rotation)
        {
            var scopedInstance = _parentScope.CreateChildFromPrefab(_prefabScope);
            try
            {
                var gk = scopedInstance.Container.Resolve<GhostKing>();
                gk.transform.SetPositionAndRotation(position, rotation);
                return gk;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to resolve GhostKing: {e}");
                throw;
            }
        }
    }
}
