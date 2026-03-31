using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Nobeta.Sam.Enemy
{
    public class Barrier : MonoBehaviour
    {
        private MeshRenderer _mr;
        private MaterialPropertyBlock _propBlock;
        private static readonly int DissolveAmountId = Shader.PropertyToID("_Dissolve");
        private CancellationTokenSource _cts;
        private Collider _collider;
        private void Awake()
        {
            _mr = GetComponent<MeshRenderer>();
            _collider = GetComponent<Collider>();
            _collider.enabled = true;

        }

        public void Show()
        {
            _collider.enabled = true;
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            PlayActivation(0.8f).Forget();
        }

        public void Hide()
        {
            _collider.enabled = false;
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            PlayDeActivation(0.8f).Forget();
        }



        public async UniTaskVoid PlayActivation(float duration)
        {
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(1f - (elapsed / duration)); // ¡ö starts at 1, goes to 0
                _mr.GetPropertyBlock(_propBlock);
                _propBlock.SetFloat(DissolveAmountId, progress);
                _mr.SetPropertyBlock(_propBlock);
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            }
            // Ensure it lands exactly at 0
            _mr.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(DissolveAmountId, 0f);
            _mr.SetPropertyBlock(_propBlock);
        }

        public async UniTaskVoid PlayDeActivation(float duration)
        {
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);

                _mr.GetPropertyBlock(_propBlock);
                _propBlock.SetFloat(DissolveAmountId, progress);
                _mr.SetPropertyBlock(_propBlock);

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
            }
        }
    }
}
