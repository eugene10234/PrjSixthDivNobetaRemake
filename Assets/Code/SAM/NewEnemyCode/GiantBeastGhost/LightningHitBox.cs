using Cysharp.Threading.Tasks;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.Sam.Enemy;
using System;
using UnityEngine;

public class LightningHitBox : MonoBehaviour
{
    private Collider col;
    [SerializeField] private GameObject ligntningEffect;
    private IEventBus _eventbus;

    private void Awake()
    {
        col = GetComponent<Collider>();
        _eventbus = EventBusProvider.GetEventBus();
        col.enabled = false;
        ligntningEffect.SetActive(false);
    }

    private void Start()
    {
        Invoke(nameof(EnableEffect), 2f);
        Destroy(gameObject, 2.5f);
    }

    private void EnableEffect()
    {
        ligntningEffect.SetActive(true);
        col.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            _eventbus.Publish(new PlayerDamagedEvent
            {
                Damage = 50,
                hitPosition = transform.position
            });
        }
    }
}
