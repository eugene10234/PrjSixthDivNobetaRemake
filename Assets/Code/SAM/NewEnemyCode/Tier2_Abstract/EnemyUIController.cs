using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;


namespace Nobeta.Sam.Enemy
{
    public class EnemyUIController : MonoBehaviour, IStartable
    {
        [SerializeField] private Image _healthBarFill;
        private CancellationTokenSource _animateCts;

        [Inject] 
        private BaseEnemyData _data;

        public void Construct(BaseEnemyData data)
        {
            _data = data;
        }

        void IStartable.Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
            BindUI(token).Forget();
        }

        private async UniTaskVoid BindUI(CancellationToken token)
        {
            await foreach (var latestPercent in _data.HealthPercent.WithCancellation(token))
            {
                // cancel previous animation
                _animateCts?.Cancel();
                _animateCts?.Dispose();

                _animateCts = CancellationTokenSource.CreateLinkedTokenSource(token);

                AnimateFill(latestPercent, _animateCts.Token).Forget();
            }
        }

        private async UniTask AnimateFill(float target, CancellationToken token)
        {
            if (target <= 0f)
            {
                _healthBarFill.fillAmount = 0f;
            }
            float start = _healthBarFill.fillAmount;
            float elapsed = 0;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                _healthBarFill.fillAmount = Mathf.Lerp(start, target, elapsed / 0.2f);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            _healthBarFill.fillAmount = target;

        }
    }

}
