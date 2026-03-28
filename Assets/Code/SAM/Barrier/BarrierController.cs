using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Drives the Barrier shader's _Dissolve and _Visibility properties.
    ///
    /// Two separate mechanisms — use the right one for the right job:
    ///
    ///   _Dissolve  (0→1) — noise-clips geometry away with edge glow.
    ///                       Use for the gate-open animation: dramatic,
    ///                       the barrier physically dissolves out of existence.
    ///
    ///   _Visibility (0→1) — multiplies master alpha, no clip, no glow.
    ///                        Use for instant hide/show without any effect,
    ///                        e.g. disabling a barrier that was never activated.
    ///
    /// Gate logic:
    ///   - Tracks registered enemy GameObjects in its area
    ///   - When all die → Open() → barrier dissolves away
    ///   - Close() → barrier dissolves back in (reset for reuse if needed)
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class BarrierController : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Gate")]
        [SerializeField] private bool  _startClosed    = true;
        [SerializeField] private float _dissolveOutTime = 1.5f;  // open:  barrier dissolves away
        [SerializeField] private float _dissolveInTime  = 0.8f;  // close: barrier dissolves back

        [Header("Optional SFX")]
        [SerializeField] private AudioClip   _openClip;
        [SerializeField] private AudioClip   _closeClip;
        [SerializeField] private AudioSource _audioSource;

        // ── Shader property IDs ───────────────────────────────────────────────

        private static readonly int DissolveId   = Shader.PropertyToID("_Dissolve");
        private static readonly int VisibilityId = Shader.PropertyToID("_Visibility");

        // ── Runtime ───────────────────────────────────────────────────────────

        private Renderer             _renderer;
        private MaterialPropertyBlock _mpb;

        private readonly HashSet<GameObject> _trackedEnemies = new();
        private float _currentDissolve;
        private bool  _isOpen;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _mpb      = new MaterialPropertyBlock();

            if (_startClosed)
            {
                SetDissolveImmediate(0f);   // fully visible, no dissolve
                SetVisibilityImmediate(1f);
            }
            else
            {
                SetVisibilityImmediate(0f); // instantly hidden, skip dissolve
                _isOpen = true;
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Register an enemy that must die before this gate opens.
        /// Called by EnemySpawnPoint after activating the enemy.
        /// </summary>
        public void RegisterEnemy(GameObject enemy)
        {
            _trackedEnemies.Add(enemy);
        }

        /// <summary>
        /// Call from EnemySpawnPoint's OnDeath subscription.
        /// When the last registered enemy dies the gate opens automatically.
        /// </summary>
        public void UnregisterEnemy(GameObject enemy)
        {
            _trackedEnemies.Remove(enemy);
            CheckIfCleared();
        }

        /// <summary>Dissolve the barrier away (gate opens).</summary>
        public void Open()
        {
            if (_isOpen) return;
            _isOpen = true;
            PlayClip(_openClip);
            FadeDissolve(target: 1f, duration: _dissolveOutTime,
                         this.destroyCancellationToken).Forget();
        }

        /// <summary>Dissolve the barrier back in (gate closes / resets).</summary>
        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;
            PlayClip(_closeClip);
            FadeDissolve(target: 0f, duration: _dissolveInTime,
                         this.destroyCancellationToken).Forget();
        }

        /// <summary>Instantly hide without any dissolve effect.</summary>
        public void ForceHide()
        {
            SetVisibilityImmediate(0f);
            _isOpen = true;
        }

        /// <summary>Instantly show without any dissolve effect.</summary>
        public void ForceShow()
        {
            SetDissolveImmediate(0f);
            SetVisibilityImmediate(1f);
            _isOpen = false;
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void CheckIfCleared()
        {
            if (_trackedEnemies.Count == 0 && !_isOpen)
                Open();
        }

        private void SetDissolveImmediate(float value)
        {
            _currentDissolve = value;
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(DissolveId, _currentDissolve);
            _renderer.SetPropertyBlock(_mpb);
        }

        private void SetVisibilityImmediate(float value)
        {
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(VisibilityId, value);
            _renderer.SetPropertyBlock(_mpb);
        }

        private async UniTaskVoid FadeDissolve(float target, float duration,
                                               System.Threading.CancellationToken token)
        {
            float start   = _currentDissolve;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (token.IsCancellationRequested) return;

                elapsed          += Time.deltaTime;
                float t           = Mathf.Clamp01(elapsed / duration);

                // Ease in-out so the dissolve accelerates then decelerates
                float tSmooth     = t * t * (3f - 2f * t);
                _currentDissolve  = Mathf.Lerp(start, target, tSmooth);

                _renderer.GetPropertyBlock(_mpb);
                _mpb.SetFloat(DissolveId, _currentDissolve);
                _renderer.SetPropertyBlock(_mpb);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            SetDissolveImmediate(target);
        }

        private void PlayClip(AudioClip clip)
        {
            if (_audioSource == null || clip == null) return;
            _audioSource.PlayOneShot(clip);
        }
    }
}
