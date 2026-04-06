using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class SGRune : MonoBehaviour
{
    private MeshRenderer _mr;
    private MaterialPropertyBlock _propBlock;
    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
    private static readonly int DissolveId = Shader.PropertyToID("_Dissolve");
    private static readonly int SpinSpeedID = Shader.PropertyToID("_RotationSpeed");
    private CancellationTokenSource _cts;
    private float deathduration = 0.8f;
    private void Awake()
    {
        _mr = GetComponent<MeshRenderer>();
        Hide();
    }

    public void Show()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        PlayActivation(0.3f, _cts.Token).Forget();
    }

    public void Hide()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        PlayDeActivation(0.3f, _cts.Token).Forget();
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

    public async UniTaskVoid PlayActivation(float duration, CancellationToken token)
    {
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration); 
            _mr.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(AlphaId, progress);
            _propBlock.SetFloat(SpinSpeedID, elapsed);
            _mr.SetPropertyBlock(_propBlock);
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
        }
    }

    public async UniTaskVoid PlayDeActivation(float duration, CancellationToken token)
    {
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(1-(elapsed / duration));

            _mr.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(AlphaId, progress);
            _propBlock.SetFloat(SpinSpeedID, (3 - elapsed));
            _mr.SetPropertyBlock(_propBlock);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
        }
        // Ensure it lands exactly at 0
        _mr.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(AlphaId, 0f);
        _mr.SetPropertyBlock(_propBlock);
    }
}

