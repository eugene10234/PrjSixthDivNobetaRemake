using Assets.Code.EUG.AANEW.Temp;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;

public class StaffHitBox : MonoBehaviour
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
        if(other.gameObject.layer==LayerMask.NameToLayer("Enemy"))
        {
            // µo¥¬¨Æ¥ó
            IDamageable target;
            if (EnemyDamageSystem.TryGetTarget(other.gameObject, out target))
            {
                target.TakeDamage(99999, transform.position);
                // AudioClip clip = Resources.Load<AudioClip>($"Audio/SFX/{sfxName}");
                // AudioSource.PlayClipAtPoint(clip, Vector3.zero, volume);Assets/MasterMagicFX/ParticlesVer3/Cosmos/CosmoShoot2/Prefabs/Par_CosmoShootMuzzle.prefab

            }
        }
    }
}   
