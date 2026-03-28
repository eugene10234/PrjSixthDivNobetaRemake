using Assets.Code.EUG.AANEW.Temp;
using Assets.Code.EUG.MBFunctions.GUI;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class StaffHitBox : MonoBehaviour
{
    private Collider hitarea;
    private IEventBus _eventBus;
    private float _damageHolder;
    private Vector3 _charPos;

    private void Awake()
    {
        _eventBus = EventBusProvider.GetEventBus();
        hitarea = GetComponent<Collider>();
        if (hitarea != null)
        {
            hitarea.enabled = false;
        }
    }
    private void OnEnable()
    {
        // 訂閱事件
        _eventBus.Subscribe<MeleeAttackEvent>(OnMeleeAttack);
        _eventBus.Subscribe<MeleeEndEvent>(OnMeleeEnd);
    }

    private void OnDisable()
    {
        // 取消訂閱
        _eventBus.Unsubscribe<MeleeAttackEvent>(OnMeleeAttack);
        _eventBus.Unsubscribe<MeleeEndEvent>(OnMeleeEnd);
    }
    private void OnMeleeAttack(MeleeAttackEvent evt)
    {
        if (hitarea == true)
        {
            hitarea.enabled = false;
        }
        _damageHolder = evt.Damage;
        _charPos = evt.AttackPosition;
        hitarea.enabled = true;
    }
    private void OnMeleeEnd(MeleeEndEvent evt)
    {
        hitarea.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer==LayerMask.NameToLayer("Enemy"))
        {
            // 發布事件
            IDamageable target;
            if (EnemyDamageSystem.TryGetTarget(other.gameObject, out target))
            {
                target.TakeDamage(_damageHolder, _charPos);
                // AudioClip clip = Resources.Load<AudioClip>($"Audio/SFX/{sfxName}");
                // AudioSource.PlayClipAtPoint(clip, Vector3.zero, volume);Assets/MasterMagicFX/ParticlesVer3/Cosmos/CosmoShoot2/Prefabs/Par_CosmoShootMuzzle.prefab

            }
        }
    }
}   
