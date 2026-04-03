using Cysharp.Threading.Tasks;
using Nobeta.EUG.GameFlowTry.Constants;
using Nobeta.EUG.GameFlowTry.Core.EventBus;
using Nobeta.EUG.GameFlowTry.Core.GameEvents;
using Nobeta.EUG.GameFlowTry.Game;
using Nobeta.EUG.GameFlowTry.UI;
using Nobeta.EUG.Interfaces;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Nobeta.EUG.GameFlowTry.GameScene
{
    /// <summary>
    /// GameScene 邏輯控制器（純 C# EntryPoint）。
    /// DrawDebugInfo() 顯示當前場景、SpawnData、存檔狀態 overlay。
    /// </summary>
    public class GameSceneController : IStartable, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly IGameManager _gameManager;
        private readonly PlayerTeleporter _player;
        private readonly GameHudView _hud;
        private readonly CutscenePlayer _cutscene;

        // debug state
        private string _debugStatus = "Initializing...";
        private string _lastSaveInfo = "—";
        private bool _initCompleted;

        private readonly SceneSpawnPoint _sceneSpawnPoint; // ✅ 新增

        public GameSceneController(
            IEventBus eventBus, IGameManager gameManager,
            PlayerTeleporter player, GameHudView hud,
            CutscenePlayer cutscene,
            SceneSpawnPoint sceneSpawnPoint) // ✅ 新增
        {
            _eventBus = eventBus;
            _gameManager = gameManager;
            _player = player;
            _hud = hud;
            _cutscene = cutscene;
            _sceneSpawnPoint = sceneSpawnPoint; // ✅
        }

        public void Start()
        {
            _cutscene.Inject(_eventBus);
            _hud.OnSaveClicked += OnSave;
            _hud.OnMenuClicked += OnMenu;
            _hud.OnChangeSceneClicked += OnChangeScene;
            _eventBus.Subscribe<GameSavedEvent>(OnGameSaved);

            GameDebugProxy.Create(this);
            InitializeAsync().Forget();
        }
        private void OnChangeScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == SceneNames.Game1)
                _eventBus.Publish(new LoadSceneRequestedEvent(SceneNames.Game2));
            else
                _eventBus.Publish(new LoadSceneRequestedEvent(SceneNames.Game1));
        }
        private async UniTaskVoid InitializeAsync()
        {
            var spawn = _gameManager.PendingSpawn;

            if (spawn == null)
            {
                _debugStatus = "⚠ PendingSpawn is null, using SceneSpawnPoint fallback";
                _player.Teleport(_sceneSpawnPoint.Position, _sceneSpawnPoint.Rotation);
                return;
            }

            if (spawn.UseSceneDefault)
            {
                // ✅ New Game 或切 Scene → 用 Scene 自己的出生點
                _player.Teleport(_sceneSpawnPoint.Position, _sceneSpawnPoint.Rotation);
                _debugStatus = $"Teleported (Scene Default) → {_sceneSpawnPoint.Position:F2}";
            }
            else
            {
                // ✅ Continue → 用存檔的 Transform
                _player.Teleport(spawn.Position, spawn.Rotation);
                _debugStatus = $"Teleported (Save) → {spawn.Position:F2}";
            }

            if (spawn.PlayIntroCutscene)
            {
                _debugStatus = "Playing intro cutscene...";
                await _cutscene.PlayAsync("intro");
            }

            _initCompleted = true;
            _debugStatus = "Ready";
            _hud.ShowStatus($"Scene: {SceneManager.GetActiveScene().name}");
        }

        private void OnSave()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            _lastSaveInfo = $"{sceneName} @ {_player.Position:F2}";

            // ✅ 同時傳 Rotation
            _eventBus.Publish(new SaveGameRequestedEvent(
                sceneName,
                _player.Position,
                _player.Rotation   // ✅ 新增
            ));
        }

        private void OnMenu() => _eventBus.Publish(new ReturnToMenuRequestedEvent());

        private void OnGameSaved(GameSavedEvent _)
        {
            _hud.ShowStatus("✔ 存檔完成！");
            _debugStatus = $"Saved: {_lastSaveInfo}";
        }

        // ── DrawDebugInfo ─────────────────────────────────────────────────

        /// <summary>由 GameDebugProxy 每幀呼叫。</summary>
        // Scene View 用 — 在 Update 呼叫
        public void DrawDebugGizmos()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var spawn = _gameManager.PendingSpawn;
            if (spawn != null)
                Debug.DrawLine(spawn.Position, _player.Position, Color.white);
#endif
        }

        // Game View 用 — 在 OnGUI 呼叫
        public void DrawDebugGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var spawn = _gameManager.PendingSpawn;
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(10, 220, 320, 100), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var bold = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };
            var normal = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            var cyan = new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = Color.cyan } };
            var green = new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = Color.green } };

            GUI.Label(new Rect(18, 224, 300, 22), "🎮 [GAME DEBUG]", bold);
            GUI.Label(new Rect(18, 246, 300, 20), $"Scene: {SceneManager.GetActiveScene().name}  Status: {_debugStatus}", normal);
            GUI.Label(new Rect(18, 264, 300, 20),
                spawn != null ? $"Spawn: {spawn.Position:F2}  Cutscene: {spawn.PlayIntroCutscene}" : "Spawn: null", cyan);
            GUI.Label(new Rect(18, 282, 300, 20),
                $"Init: {(_initCompleted ? "✔ Done" : "⏳ Wait")}  LastSave: {_lastSaveInfo}", green);
            GUI.Label(new Rect(18, 300, 300, 20), $"PlayerPos: {_player.Position:F2}", normal);
#endif
        }

        public void Dispose()
        {
            _hud.OnSaveClicked -= OnSave;
            _hud.OnMenuClicked -= OnMenu;
            _hud.OnChangeSceneClicked -= OnChangeScene;
            _eventBus.Unsubscribe<GameSavedEvent>(OnGameSaved);
        }
    }

    // ── Proxy MonoBehaviour ───────────────────────────────────────────────

    internal class GameDebugProxy : MonoBehaviour
    {
        private GameSceneController _controller;

        public static void Create(GameSceneController controller)
        {
            var go = new GameObject("[DEBUG] GameDebugProxy");
            var proxy = go.AddComponent<GameDebugProxy>();
            proxy._controller = controller;
        }

        // ✅ Debug.DrawLine/Ray 只在 Update 呼叫
        private void Update() => _controller?.DrawDebugGizmos();

        // ✅ GUI.* 只在 OnGUI 呼叫
        private void OnGUI() => _controller?.DrawDebugGUI();
    }
}
