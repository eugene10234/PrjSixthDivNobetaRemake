using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.Sam.Enemy;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    private BoxCollider hitarea;
    private IEventBus _eventBus;
    public EnemyStat enemyStat;
    private void Awake()
    {
        _eventBus = EventBusProvider.GetEventBus();
        hitarea = GetComponent<BoxCollider>();
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
            _eventBus.Publish(new PlayerDamagedEvent
            {
                Damage = Mathf.FloorToInt(enemyStat.AttackPower),       
                hitPosition = transform.position
            });
        }
    }
}   
