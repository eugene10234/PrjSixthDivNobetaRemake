using System;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    public abstract class EnemySpawner<EnemyType> : MonoBehaviour, VContainer.Unity.IStartable
    {
        private IEnemyFactory<EnemyType> _enemyFactory;

        [Inject]
        public void Construct(IEnemyFactory<EnemyType> factory)
        {
            _enemyFactory = factory;
        }


        void VContainer.Unity.IStartable.Start()
        {
            OnReady();
        }

        protected abstract void OnReady();

        public void Create(Vector3 position, Quaternion Rotation)
        {
            if (_enemyFactory == null)
            {
                Debug.LogError($"[Create] failed! _enemyFactory is still null on {gameObject.name}.");
                return;
            }

            //if (Physics.Raycast(position, Vector3.down, out RaycastHit hitinfo, 100.0f))
            //{
            //    position = hitinfo.point;
            //}

            _enemyFactory.Create(position, Rotation);
        }
    }
}
