using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// MageGhost animation actor — owns the Animator and SkinnedMeshRenderer.
    /// Implements IAnimationActor so states never touch Animator directly.
    ///
    /// Registered via RegisterComponent().As&lt;IAnimationActor&gt;() in MGLifetimeScope.
    /// </summary>
    public class MGAnimationActor : MonoBehaviour, IAnimationActor
    {
        private Animator _animator;
        private SkinnedMeshRenderer _smr;
        private MaterialPropertyBlock _propBlock;
        private HitBox _hitbox;

        private static readonly int DissolveAmountId = Shader.PropertyToID("_Dissolve");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _smr = GetComponentInChildren<SkinnedMeshRenderer>();
            _hitbox = GetComponentInChildren<HitBox>();
        }

        // ── IAnimationActor ───────────────────────────────────────────────────

        public void PlayHit()
        {
            _animator.SetTrigger("tHit");
        }

        public UniTask PlayAttack()
        {
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

        public void PlayVictory()
        {
            _animator.SetBool("isVictory", true);
        }

        public void PlayDeath()
        {
            _animator.SetBool("isDead", true);
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
