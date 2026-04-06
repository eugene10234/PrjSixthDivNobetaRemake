using Cysharp.Threading.Tasks;
using Nobeta.Sam.Enemy;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer.Unity;
using static Coffee.UIExtensions.UIParticleAttractor;

namespace Nobeta.Sam.Enemy
{
    public class GKAnimationActor : MonoBehaviour, IAnimationActor
    {
        private static readonly int DissolveAmount = Shader.PropertyToID("_Dissolve");
        private static readonly int EmissionColour = Shader.PropertyToID("_EmissionColor");
        private MaterialPropertyBlock _propBlock;
        private Animator _animator;
        private SkinnedMeshRenderer _smr;
        private CancellationTokenSource _animateCts;
        public GameObject SmokeFX;
        public GameObject WindTrailFX;
        public SGRune FireRune;
        public MGRune LaserRune;
        private GKHitBox_Tick _tick;
        public HitBox hitbox;
        public GameObject lightningPrefab;
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _smr = GetComponentInChildren<SkinnedMeshRenderer>();
            _tick = GetComponentInChildren<GKHitBox_Tick>();
        }

        public void ShowAngryFace()
        {
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
            _smr.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(EmissionColour, new Color(1f, 0f, 0f));
            _smr.SetPropertyBlock(_propBlock);
        }

        public async UniTask InstantiateLightning(Vector3 loc)
        {
            loc.y = -2f;
            var lightning = Instantiate(lightningPrefab, loc, lightningPrefab.transform.rotation);
            await UniTask.Delay(500);
        }
        public void ActivateWind()
        { 
            WindTrailFX.SetActive(true);
        }
        public void ActivateSmoke()
        { 
            SmokeFX.SetActive(true);
        }
        public async UniTask<bool> StartPreMeleeAttackMotion(CancellationToken token)
        {
            ActivateSmoke();
            _animator.SetBool("isPreAttacking", true);
            await UniTask.Delay(500, cancellationToken: token);
            ActivateWind();
            await UniTask.Delay(2000, cancellationToken: token);
            bool success = true;
            return success;
        }

        public void EndPreAttack()
        {
            _animator.SetBool("isPreAttacking", false);
        }
        public void StartWindAttack()
        {
            _animator.ResetTrigger("tAttack");
            _animator.SetTrigger("tAttack");
        }

        public void AE_DeactivateWindFX()
        {
            WindTrailFX.SetActive(false);
            SmokeFX.SetActive(false);
        }

        public void AE_ActivateHitBox()
        {
            hitbox.ActivateHitArea();
        }

        public void AE_DeactivateHitBox()
        {
            hitbox.DeActivateHitArea();
        }

        public async UniTask<bool> ActivateFireRune(CancellationToken token)
        {
            FireRune.Show();
            await UniTask.Delay(1000, cancellationToken: token);
            bool success = true;
            return success;
        }
        public async UniTask<bool> ActivateLaserRune(CancellationToken token = default)
        {
            LaserRune.Show();
            await UniTask.Delay(500, cancellationToken: token);
            bool success = true;
            return success;
        }

        public async UniTaskVoid ActivateLaser(CancellationToken token)
        {

            try
            {
                _tick.AttackActivation(); // Start the visual/logic

                // Pass the token to the delay. If cancelled during these 1000ms, 
                // the method stops RIGHT HERE.
            }
            catch (OperationCanceledException)
            {
                // This block runs if the token was cancelled.
                // Clean up your visuals here (e.g., turn off the laser effect).
                Debug.Log("Laser Activation was cancelled.");
                _tick.EndAttackFX();
            }
        }

        public void DeactivateLaser()
        {
            _tick.EndAttackFX();
            LaserRune.Hide();
        }
        public void DeactivateFireRune()
        {
            FireRune.Hide();
        }
        

        public void SetCatatonic()
        {
            _animator.SetBool("isCatatonic", true);
        }

        public void ResetCatatonic()
        {
            _animator.SetBool("isCatatonic", false);
        }
        public UniTask PlayAttack()
        {
            //not called
            throw new System.NotImplementedException();
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
                _propBlock.SetFloat(DissolveAmount, progress);
                _smr.SetPropertyBlock(_propBlock);

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        public void PlayHit()
        {
           
        }

        public void PlayVictory()
        {
            _animator.SetTrigger("isVictory");
        }

        
    }
}