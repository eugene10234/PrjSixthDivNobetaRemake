using Assets.Code.EUG.AANEW.Temp;
using Nobeta.EUG.BulletSystem.OOP;
//using Nobeta.EUG.Core.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;
using VContainer.Unity;
// ========================================================================
// PhysicsService (物理服務實現)
// ========================================================================
namespace Nobeta.EUG.BulletSystem.OOP.UnityBridge
{

    /// <summary>
    /// 物理服務（封裝 Unity Physics）
    /// </summary>
    public class PhysicsService : IPhysicsService
    {
        public CollisionHit OverlapSphere(Vector3 position, float radius, int layerMask)
        {
            Collider[] hits = Physics.OverlapSphere(position, radius, layerMask);

            if (hits.Length > 0)
            {
                //IDamageable target;
                //if (DamageSystem.TryGetTarget(hits[0], out target))
                //{
                //    target.TakeDamage(50, position);
                //    // AudioClip clip = Resources.Load<AudioClip>($"Audio/SFX/{sfxName}");
                //    // AudioSource.PlayClipAtPoint(clip, Vector3.zero, volume);Assets/MasterMagicFX/ParticlesVer3/Cosmos/CosmoShoot2/Prefabs/Par_CosmoShootMuzzle.prefab

                //}
                return new CollisionHit
                {
                    HasHit = true,
                    GameObject = hits[0].gameObject,
                    //Normal = (position - hits[0].transform.position).normalized                    
                };
            }

            return new CollisionHit { HasHit = false };
        }
        public CollisionHit SphereCast(Vector3 position, float radius , Vector3 direction, float distance, int layerMask)
        {
            //Collider[] hits = Physics.SphereCast(position, radius, direction, out hit, distance, layerMask);
            RaycastHit hit;
            if (Physics.SphereCast(position, radius, direction, out hit, distance, layerMask))
            {
                
                //IDamageable target;
                //if (DamageSystem.TryGetTarget(hits[0], out target))
                //{
                //    target.TakeDamage(50, position);
                //    // AudioClip clip = Resources.Load<AudioClip>($"Audio/SFX/{sfxName}");
                //    // AudioSource.PlayClipAtPoint(clip, Vector3.zero, volume);Assets/MasterMagicFX/ParticlesVer3/Cosmos/CosmoShoot2/Prefabs/Par_CosmoShootMuzzle.prefab

                //}
                return new CollisionHit
                {
                    HasHit = true,
                    GameObject = hit.collider.gameObject,
                    //Normal = (position - hits[0].transform.position).normalized                    
                };
            }

            return new CollisionHit { HasHit = false };
        }
    }
}


