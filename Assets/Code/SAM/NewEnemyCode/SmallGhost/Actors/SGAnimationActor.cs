using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Nobeta.Sam.Enemy
{
    public class SGAnimationActor : MonoBehaviour, IAnimationActor
    {
        private Animator _animator;
        private SkinnedMeshRenderer _smr;
        private MaterialPropertyBlock _propBlock;


        private static readonly int DissolveAmountId = Shader.PropertyToID("_Dissolve");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _smr = GetComponentInChildren<SkinnedMeshRenderer>();
        }

        // ¢w¢w IAnimationActor ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w

        public void PlayHit()
        {
            _animator.SetTrigger("tHit");
        }

        public UniTask PlayAttack()
        {
            _animator.SetTrigger("tAttack");
            return UniTask.CompletedTask;
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

            float elapsed = 1.5f;
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
        public async UniTask PlayPreAttack(CancellationToken token)
        {
            
            
        }
    }
}


