using Cysharp.Threading.Tasks;
using UnityEngine;

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

        private MGRune _mgRune;

        private static readonly int DissolveAmountId = Shader.PropertyToID("_Dissolve");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _smr = GetComponentInChildren<SkinnedMeshRenderer>();

            _mgRune = GetComponentInChildren<MGRune>();
        }

        // ── IAnimationActor ───────────────────────────────────────────────────

        public void PlayHit()
        {
            _animator.SetTrigger("tHit");
        }

        public void PlaySleep()
        {
            _animator.SetBool("isSleep", true);
        }

        public void PlayAwake()
        {
            _animator.SetBool("isSleep", false);
        }

        public void PlaySummon()
        {
            _animator.SetBool("isSummoning", true);
        }

        public void AE_PlayFinsihSummon()
        {
            _animator.SetBool("isSummoning", false);
        }
        public UniTask PlayAttack()
        {
            //_hitboxtick.StartAttackFX().Forget();
            return UniTask.CompletedTask;
        }

        public async UniTask PlayPreAttack()
        {
            _mgRune.Show();
            await UniTask.Delay(3000);
        }

        public async UniTask StartLaser()
        {

            await UniTask.Delay(3000);
        }

        public void HideRune()
        {
            _mgRune.Hide();
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
            HideRune();
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
