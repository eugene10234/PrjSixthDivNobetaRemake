using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Nobeta.Sam.Enemy
{
    public class FDAnimationActor: MonoBehaviour, IAnimationActor
    {
        private Animator _animator;
        private SkinnedMeshRenderer _smr;
        private MaterialPropertyBlock _propBlock;
        private HitBox_Tick _hitboxtick;

        private static readonly int DissolveAmountId = Shader.PropertyToID("_Dissolve");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _smr = GetComponentInChildren<SkinnedMeshRenderer>();
            _hitboxtick = GetComponentInChildren<HitBox_Tick>();
        }

        // ── IAnimationActor ───────────────────────────────────────────────────

        public void PlayHit()
        {
            _animator.SetTrigger("tHit");
        }

        public UniTask PlayAttack()
        {
            return BreatheFire(this.destroyCancellationToken);
        }

        private float _breatheFireTimer;
        private float _breatheFireDuration = 5;
        private async UniTask BreatheFire(CancellationToken token)
        {
            _breatheFireTimer = 0; // Reset at start to be safe
            _hitboxtick.StartBreatheFireFX();

            try
            {
                while (_breatheFireTimer < _breatheFireDuration)
                {
                    // Match deltaTime to the Yield timing!
                    _breatheFireTimer += Time.deltaTime;

                    // This 'token' stops the loop if the object is destroyed
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (System.OperationCanceledException) { }
            finally
            {
                // 'finally' ensures the hitbox turns off even if the task is cancelled
                _hitboxtick.EndBreatheFireFX();
                _breatheFireTimer = 0;
            }
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
