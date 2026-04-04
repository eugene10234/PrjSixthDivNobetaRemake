using Cysharp.Threading.Tasks;
using Nobeta.EUG.GameFlow.Constants;
using Nobeta.EUG.GameFlow.Core.EventBus;
using Nobeta.EUG.GameFlow.Core.GameEvents;
using Nobeta.EUG.GameFlow.Core.Save;
using Nobeta.EUG.GameFlow.Core.Scene;
using Nobeta.EUG.Interfaces;
using System;
using UnityEngine;

namespace Nobeta.EUG.GameFlow.Game
{
    public sealed class GameManager : IGameManager, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly ISaveSystem _saveSystem;
        private readonly ISceneLoader _sceneLoader;

        public SpawnData PendingSpawn { get; private set; }

        public GameManager(IEventBus eventBus, ISaveSystem saveSystem, ISceneLoader sceneLoader)
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
            => SaveGameAsync(e.SceneName, e.Position, e.Rotation).Forget();
        // OnLoadSceneRequested（切換 Scene 按鈕）
        private void OnLoadSceneRequested(LoadSceneRequestedEvent e)
        {
            PendingSpawn = new SpawnData
            {
                SceneName = e.SceneName,
                UseSceneDefault = true,   // ✅ 切場景時也用 Scene 預設出生點
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
                UseSceneDefault = true,   // ✅ Scene 自己提供出生點，GameManager 不寫死
                PlayIntroCutscene = true
            };

            await _sceneLoader.LoadSceneAsync(SceneNames.Game1);
        }
        public async UniTask ContinueGameAsync()
        {
            var save = _saveSystem.Load();
            if (save == null) { await StartNewGameAsync(); return; }

            PendingSpawn = new SpawnData
            {
                SceneName = save.SceneName,
                UseSceneDefault = false,            // ✅ 使用存檔的 Transform
                Position = save.PlayerPosition,
                Rotation = save.PlayerRotation, // ✅
                PlayIntroCutscene = false
            };

            await _sceneLoader.LoadSceneAsync(save.SceneName);
        }

        public async UniTask ReturnToMenuAsync()
        {
            PendingSpawn = null;
            await _sceneLoader.LoadSceneAsync(SceneNames.Menu);
        }

        public async UniTask SaveGameAsync(string sceneName, Vector3 position, Quaternion rotation)
        {
            var data = new SaveData { SceneName = sceneName };
            data.SetPlayerTransform(position, rotation); // ✅
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
