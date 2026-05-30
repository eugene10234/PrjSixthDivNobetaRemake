using Cysharp.Threading.Tasks;
using GDCO.Core.Entities;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.Sam.Enemy;
using UnityEngine;
using VContainer;

namespace GDCO.Combat.Integration
{
    public  class EntityMonoBridge : MonoBehaviour
    {
        public int TestInt;
        private IEventBus _eventBus;
        public virtual IEventBus EventBus => _eventBus;
        private bool isPlayerLive = true;
        public bool IsPlayerLive => isPlayerLive;
        public bool IsEventBusInitialized => _eventBus != null;

        //public void EntityProvider()
        //{
        //    _eventBus = await EventBusProvider.GetEventBusAsync();
        //}
        private void Start()
        {
            _eventBus.Subscribe<EntityRegistryEvent>(OnEntityRegistry);
        }

        void OnEnable()
        {
            _eventBus = EventBusProvider.GetEventBus();

            //_eventBus.Subscribe<PlayerLiveStateEvent>(OnPlayerLiveStateChange);
        }
        void OnDisable()
        {
            //_eventBus.Unsubscribe<PlayerLiveStateEvent>(OnPlayerLiveStateChange);
        }
        private int _entityId;

        /// <summary>
        /// 設置 Entity ID
        /// </summary>
        public void Setup(int entityId)
        {
            _entityId = entityId;
        }

        /// <summary>
        /// 取得 Entity ID
        /// </summary>
        public int EntityId => _entityId;
        private IEnemyCollisionRegistry collisionRegistry;//maybe null if not registered yet
        private void OnEntityRegistry(EntityRegistryEvent evt)
        {
            Setup(evt.entityId);
            collisionRegistry = evt.collisionRegistry;
            collisionRegistry?.Register(gameObject, EntityId);
        }
        private void OnDestroy()
        {
            collisionRegistry?.Unregister(gameObject);
            _eventBus.Unsubscribe<EntityRegistryEvent>(OnEntityRegistry);
        }
        private void OnPlayerLiveStateChange(PlayerLiveStateEvent evt)
        {
            isPlayerLive = evt.IsLive;
        }
        //public abstract void TakeDamage(float amount, Vector3 position);
    }
}
