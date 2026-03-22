using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    private Collider hitarea;
    private IEventBus _eventBus;
    private void Awake()
    {
        _eventBus = EventBusProvider.GetEventBus();
        hitarea = GetComponent<Collider>();
        if (hitarea != null)
        { 
            hitarea.enabled = false;
        }
    }

    public void ActivateHitArea()
    {
        hitarea.enabled = true;

    }

    public void DeActivateHitArea()
    {
        hitarea.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer==LayerMask.NameToLayer("Player"))
        {
            // µo¥¬¨Æ¥ó
            _eventBus.Publish(new PlayerDamagedEvent
            {
                Damage = 20,       
                hitPosition = transform.position
            });
        }
    }
}   
