using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using UnityEngine;

public class KillZoneTest : MonoBehaviour
{
    private IEventBus _eventBus;
    private void Awake()
    {
        _eventBus = EventBusProvider.GetEventBus();
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // µo¥¬¨Æ¥ó
            _eventBus.Publish(new PlayerDamagedEvent
            {
                Damage = 99999,
                hitPosition = transform.position
            });
        }
    }
}
