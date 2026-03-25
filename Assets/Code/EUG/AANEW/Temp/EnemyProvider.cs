using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using UnityEngine;

namespace Assets.Code.EUG.AANEW.Temp
{
    public abstract class EnemyProvider : MonoBehaviour, IDamageable
    {
        public int TestInt;
        private IEventBus _eventBus;
        public virtual IEventBus EventBus => _eventBus;
        private bool isPlayerLive = true;
        public bool IsPlayerLive => isPlayerLive;
        void OnEnable()
        {
            _eventBus = EventBusProvider.GetEventBus();
            EnemyDamageSystem.Targets[gameObject.GetInstanceID()] = this;
            _eventBus.Subscribe<PlayerLiveStateEvent>(OnPlayerLiveStateChange);
        }
        void OnDisable()
        {
            EnemyDamageSystem.Targets.Remove(gameObject.GetInstanceID());
            _eventBus.Unsubscribe<PlayerLiveStateEvent>(OnPlayerLiveStateChange);
        }

        private void OnPlayerLiveStateChange(PlayerLiveStateEvent evt)
        {
            isPlayerLive = evt.IsLive;
        }
        public abstract void TakeDamage(float amount, Vector3 position);
    }
}
