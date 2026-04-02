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
    public GameObject PreAttackFX;
    public GameObject AttackFX;
    private void Awake()
    {
        _eventBus = EventBusProvider.GetEventBus();
        hitarea = GetComponent<BoxCollider>();
        if (hitarea != null)
        {
            hitarea.isTrigger = true;
            hitarea.enabled = false;
        }
        AttackFX.SetActive(false);
    }

    public async UniTask StartAttackFX()
    {
       bool succes = await StartAttackSequence(this.destroyCancellationToken);
        if (succes)
        {
            AttackActivation();
        }
    }

    private void AttackActivation()
    {
        AttackFX.SetActive(false);
        AttackFX.SetActive(true);
        hitarea.enabled = true;
    }
    private async UniTask <bool> StartAttackSequence(CancellationToken token)
    {
        try
        {
            PreAttackFX.SetActive(true);
            await UniTask.Delay(500, cancellationToken: token);
            return true;
        }
        catch (OperationCanceledException)
        {
            // Handle cleanup if the enemy dies/action is interrupted mid-way
            EndPreAttackFX();
            EndAttackFX();
            hitarea.enabled = false;
            return false;
        }
    }

    public void EndAttackFX()
    {
        AttackFX.SetActive(false);
        hitarea.enabled = false;
    }

    public void EndPreAttackFX()
    {
        PreAttackFX.SetActive(false);
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