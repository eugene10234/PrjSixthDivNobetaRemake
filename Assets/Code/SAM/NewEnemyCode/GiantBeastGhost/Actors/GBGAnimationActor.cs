using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// GiantBeastGhost animation actor — owns the Animator and SkinnedMeshRenderer.
    /// Implements IAnimationActor so states never touch Animator directly.
    ///
    /// Registered via RegisterComponent().As&lt;IAnimationActor&gt;() in GBGLifetimeScope.
    /// </summary>
    public class GBGAnimationActor : MonoBehaviour, IAnimationActor
    {  
        private Animator _animator;
        private SkinnedMeshRenderer _smr;
        private MaterialPropertyBlock _propBlock;
        private HitBox _hitbox;
        public GameObject GlowHorn;
        public GameObject SmokeFX;
        public GameObject WindTrailFX;
        public GameObject DodgeFX;
        //public GameObject AttackLight;
        //public GameObject AttackFX;
        private static readonly int DissolveAmountId = Shader.PropertyToID("_Dissolve");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _smr = GetComponentInChildren<SkinnedMeshRenderer>();
            _hitbox = GetComponentInChildren<HitBox>();
            //AttackLight.SetActive(false);
            GlowHorn.SetActive(false);
            SmokeFX.SetActive(false);
            WindTrailFX.SetActive(false);
            DodgeFX.SetActive(false);
        }

        // ── IAnimationActor ───────────────────────────────────────────────────

        public void PlayHit()
        {
            _animator.speed = 1f;
            _animator.SetTrigger("tHit");
        }

        public void PlayDodge()
        {
            _animator.speed = 1f;
            _animator.SetTrigger("tDodge");
        }
        public void ActivateGlowHorn()
        {
            GlowHorn.SetActive(true);
        }

        public void DeactivateGlowHorn() 
        {
            GlowHorn.SetActive(false);
        }
        public UniTask PlayAttack()
        {
            SmokeFX.SetActive(false);
            WindTrailFX.SetActive(false);
            SmokeFX.SetActive(true);
            _animator.speed = 0.2f;
            _animator.SetTrigger("tAttack");
            return UniTask.CompletedTask;
        }

        public void AE_ActivateHitBox()
        {
            _hitbox.ActivateHitArea();
        }

        public void AE_DeActivateHitBox()
        {
            
            _hitbox.DeActivateHitArea();
        }

        public void StartFire()
        { 
            _animator.speed = 1.0f;
        }

        public event Action OnWindingFinished;
        public void AE_PausePose()
        {
            WindTrailFX.SetActive(true);
            _animator.speed = 0.05f;
            OnWindingFinished?.Invoke();
        }



        public void PlayDodgeFX()
        {
            DodgeFX.SetActive(false);
            DodgeFX.SetActive(true);
        }


        public void PlayVictory()
        {
            _animator.SetBool("isVictory", true);
        }

        public void PlayDeath()
        {
            _animator.SetBool("isDead", true);
            _hitbox.DeActivateHitArea();
        }

        public async UniTask PlayDeathSequence(float duration)
        {
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);

                _smr.GetPropertyBlock(_propBlock);
                _propBlock.SetFloat(DissolveAmountId, progress);
                _smr.SetPropertyBlock(_propBlock);

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
    }
}
