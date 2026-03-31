using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class SGRune : MonoBehaviour
{
    private MeshRenderer _mr;
    private MaterialPropertyBlock _propBlock;
    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
    private static readonly int DissolveId = Shader.PropertyToID("_Dissolve");
    private CancellationTokenSource _cts;
    private float deathduration = 0.8f;
    private void Awake()
    {
        _mr = GetComponent<MeshRenderer>();
    }

    public void Show()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        PlayActivation(0.3f).Forget();
    }

    public void Hide()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        PlayDeActivation(0.3f).Forget();
    }

    public async UniTaskVoid PlayDeathSequence()
    {
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
        float elapsed = 0f;
        while (elapsed < deathduration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / deathduration);
            _mr.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(DissolveId, progress);
            _mr.SetPropertyBlock(_propBlock);
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }
    }

    public async UniTaskVoid PlayActivation(float duration)
    {
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration); 
            _mr.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(AlphaId, progress);
            _mr.SetPropertyBlock(_propBlock);
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }
    }

    public async UniTaskVoid PlayDeActivation(float duration)
    {
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(1-(elapsed / duration));

            _mr.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(AlphaId, progress);
            _mr.SetPropertyBlock(_propBlock);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }
        // Ensure it lands exactly at 0
        _mr.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(AlphaId, 0f);
        _mr.SetPropertyBlock(_propBlock);
    }
}

