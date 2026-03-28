using Cysharp.Threading.Tasks;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.Sam.Enemy;
using System;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class HitBox_Tick : MonoBehaviour
{
    private BoxCollider hitarea;
    private IEventBus _eventBus;
    public EnemyStat enemyStat;
    public GameObject RuneFX;
    public GameObject FlameFX;
    private void Awake()
    {
        _eventBus = EventBusProvider.GetEventBus();
        hitarea = GetComponent<BoxCollider>();
        if (hitarea != null)
        {
            hitarea.isTrigger = true;
            hitarea.enabled = false;
        }
        FlameFX.SetActive(false);
    }

    public async UniTask StartBreatheFireFX()
    {
       bool succes = await RuneActivation(this.destroyCancellationToken);
        if (succes)
        {
            FireActivation();
        }
    }

    private void FireActivation()
    {
        FlameFX.SetActive(false);
        FlameFX.SetActive(true);
        hitarea.enabled = true;
    }
    private async UniTask <bool> RuneActivation(CancellationToken token)
    {
        try
        {
            RuneFX.SetActive(true);
            await UniTask.Delay(500, cancellationToken: token);
            return true;
        }
        catch (OperationCanceledException)
        {
            // Handle cleanup if the enemy dies/action is interrupted mid-way
            EndBreatheFireFX();
            hitarea.enabled = false;
            return false;
        }
    }

   

    public void EndBreatheFireFX()
    {
        RuneFX.SetActive(false);
        hitarea.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            _tickTimer = 1f;
            _eventBus.Publish(new PlayerDamagedEvent
            {
                Damage = Mathf.FloorToInt(enemyStat.AttackPower),
                hitPosition = transform.position
            });
        }
    }

    private float _tickTimer = 1;
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            _tickTimer -= Time.deltaTime;
            if (_tickTimer <= 0)
            {
                _eventBus.Publish(new PlayerDamagedEvent
                {
                    Damage = Mathf.FloorToInt(enemyStat.AttackPower),
                    hitPosition = transform.position
                });
                _tickTimer = 1f; // Reset the tick interval
            }
        }
    }
}