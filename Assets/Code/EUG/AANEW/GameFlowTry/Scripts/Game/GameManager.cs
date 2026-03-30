using Cysharp.Threading.Tasks;
using Nobeta.EUG.GameFlowTry.Constants;
using Nobeta.EUG.GameFlowTry.Core.EventBus;
using Nobeta.EUG.GameFlowTry.Core.GameEvents;
using Nobeta.EUG.GameFlowTry.Core.Save;
using Nobeta.EUG.GameFlowTry.Core.Scene;
using System;
using UnityEngine;

namespace Nobeta.EUG.GameFlowTry.Game
{
    public sealed class GameManager : IGameManager, IDisposable
    {
        private readonly IEventBus2 _eventBus;
        private readonly ISaveSystem _saveSystem;
        private readonly ISceneLoader _sceneLoader;

        public SpawnData PendingSpawn { get; private set; }

        public GameManager(IEventBus2 eventBus, ISaveSystem saveSystem, ISceneLoader sceneLoader)
        {
            _eventBus = eventBus;
            _saveSystem = saveSystem;
            _sceneLoader = sceneLoader;

            _eventBus.Subscribe<NewGameRequestedEvent>(OnNewGameRequested);
            _eventBus.Subscribe<ContinueGameRequestedEvent>(OnContinueRequested);
            _eventBus.Subscribe<ReturnToMenuRequestedEvent>(OnReturnToMenuRequested);
            _eventBus.Subscribe<SaveGameRequestedEvent>(OnSaveGameRequested);
            _eventBus.Subscribe<LoadSceneRequestedEvent>(OnLoadSceneRequested);
        }

        private void OnNewGameRequested(NewGameRequestedEvent _)
        {
             StartNewGameAsync().Forget();
        }
        private void OnContinueRequested(ContinueGameRequestedEvent _) => ContinueGameAsync().Forget();
        private void OnReturnToMenuRequested(ReturnToMenuRequestedEvent _) => ReturnToMenuAsync().Forget();
        private void OnSaveGameRequested(SaveGameRequestedEvent e)
            => SaveGameAsync(e.SceneName, e.Position).Forget();
        private void OnLoadSceneRequested(LoadSceneRequestedEvent e)
        {
            // PendingSpawn 設為目標場景的預設出生點
            PendingSpawn = new SpawnData
            {
                SceneName = e.SceneName,
                Position = e.SceneName == SceneNames.Game2
                                    ? SpawnPoints.Game2Default
                                    : SpawnPoints.Game1Default,
                PlayIntroCutscene = false
            };
            _sceneLoader.LoadSceneAsync(e.SceneName).Forget();
        }
        public async UniTask StartNewGameAsync()
        {
            _saveSystem.Delete();
            PendingSpawn = new SpawnData
            {
                SceneName = SceneNames.Game1,
                Position = SpawnPoints.Game1Default,
                PlayIntroCutscene = true
            };
            Debug.Log($"準備載入場景: {SceneNames.Game1}");
            await _sceneLoader.LoadSceneAsync(SceneNames.Game1);
        }

        public async UniTask ContinueGameAsync()
        {
            var save = _saveSystem.Load();
            if (save == null) { await StartNewGameAsync(); return; }

            PendingSpawn = new SpawnData
            {
                SceneName = save.SceneName,
                Position = save.PlayerPosition,
                PlayIntroCutscene = false
            };
            await _sceneLoader.LoadSceneAsync(save.SceneName);
        }

        public async UniTask ReturnToMenuAsync()
        {
            PendingSpawn = null;
            await _sceneLoader.LoadSceneAsync(SceneNames.Menu);
        }

        public async UniTask SaveGameAsync(string sceneName, Vector3 playerPosition)
        {
            var data = new SaveData { SceneName = sceneName };
            data.SetPlayerPosition(playerPosition);
            _saveSystem.Save(data);
            _eventBus.Publish(new GameSavedEvent());
            await UniTask.CompletedTask;
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<NewGameRequestedEvent>(OnNewGameRequested);
            _eventBus.Unsubscribe<ContinueGameRequestedEvent>(OnContinueRequested);
            _eventBus.Unsubscribe<ReturnToMenuRequestedEvent>(OnReturnToMenuRequested);
            _eventBus.Unsubscribe<SaveGameRequestedEvent>(OnSaveGameRequested);
            _eventBus.Unsubscribe<LoadSceneRequestedEvent>(OnLoadSceneRequested);
        }
    }
}
