using Cysharp.Threading.Tasks;
using Nobeta.EUG.GameFlowTry.Core.EventBus;
using Nobeta.EUG.GameFlowTry.Core.GameEvents;
using Nobeta.EUG.GameFlowTry.UI;
using UnityEngine.SceneManagement;

namespace Nobeta.EUG.GameFlowTry.Core.Scene
{
    /// <summary>
    /// allowSceneActivation = false 確保玩家看不到未就緒畫面。
    /// 進度由 LoadingScreenView 顯示，同時 Publish Event 供外部訂閱。
    /// </summary>
    public sealed class SceneLoader : ISceneLoader
    {
        private readonly IEventBus2 _eventBus;
        private readonly LoadingScreenView _loadingScreen;

        public SceneLoader(IEventBus2 eventBus, LoadingScreenView loadingScreen)
        {
            _eventBus = eventBus;
            _loadingScreen = loadingScreen;
        }

        public async UniTask LoadSceneAsync(string sceneName)
        {
            _eventBus.Publish(new SceneLoadStartedEvent(sceneName));
            _loadingScreen.Show();

            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                float normalized = op.progress / 0.9f;
                _loadingScreen.SetProgress(normalized);
                _eventBus.Publish(new SceneLoadProgressEvent(normalized));
                await UniTask.NextFrame();
            }

            _loadingScreen.SetProgress(1f);
            await UniTask.Delay(350);

            op.allowSceneActivation = true;
            await UniTask.WaitUntil(() => op.isDone);

            _loadingScreen.Hide();
            _eventBus.Publish(new SceneLoadCompletedEvent(sceneName));
        }
    }
}
