using Cysharp.Threading.Tasks;
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
    public class GameController : IStartable, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly IGameManager _gameManager;
        private readonly PlayerController _player;
        private readonly GameHudView _hud;
        private readonly CutscenePlayer _cutscene;

        // debug state
        private string _debugStatus = "Initializing...";
        private string _lastSaveInfo = "—";
        private bool _initCompleted;

        public GameController(
            IEventBus eventBus, IGameManager gameManager,
            PlayerController player, GameHudView hud, CutscenePlayer cutscene)
        {
            _eventBus = eventBus;
            _gameManager = gameManager;
            _player = player;
            _hud = hud;
            _cutscene = cutscene;
        }

        public void Start()
        {
            _cutscene.Inject(_eventBus);
            _hud.OnSaveClicked += OnSave;
            _hud.OnMenuClicked += OnMenu;
            _eventBus.Subscribe<GameSavedEvent>(OnGameSaved);

            GameDebugProxy.Create(this);
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync()
        {
            var spawn = _gameManager.PendingSpawn;

            if (spawn == null)
            {
                _debugStatus = "⚠ PendingSpawn is null, using fallback";
                _player.Teleport(Vector3.up);
                return;
            }

            _player.Teleport(spawn.Position);
            _debugStatus = $"Teleported → {spawn.Position:F2}";

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
            _eventBus.Publish(new SaveGameRequestedEvent(sceneName, _player.Position));
        }

        private void OnMenu() => _eventBus.Publish(new ReturnToMenuRequestedEvent());

        private void OnGameSaved(GameSavedEvent _)
        {
            _hud.ShowStatus("✔ 存檔完成！");
            _debugStatus = $"Saved: {_lastSaveInfo}";
        }

        // ── DrawDebugInfo ─────────────────────────────────────────────────

        /// <summary>由 GameDebugProxy 每幀呼叫。</summary>
        public void DrawDebugInfo()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var spawn = _gameManager.PendingSpawn;

            // Scene View：Spawn → Player 連線（白色）
            if (spawn != null)
                Debug.DrawLine(spawn.Position, _player.Position, Color.white);

            // ── OnGUI overlay ──────────────────────────────────────────
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(10, 170, 320, 100), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var bold = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };
            var normal = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            var cyan = new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = Color.cyan } };
            var green = new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = Color.green } };

            GUI.Label(new Rect(18, 174, 300, 22), "🎮 [GAME DEBUG]", bold);

            GUI.Label(new Rect(18, 196, 300, 20),
                $"Scene: {SceneManager.GetActiveScene().name}    Status: {_debugStatus}", normal);

            GUI.Label(new Rect(18, 214, 300, 20),
                spawn != null
                    ? $"Spawn: {spawn.Position:F2}  Cutscene: {spawn.PlayIntroCutscene}"
                    : "Spawn: null",
                cyan);

            GUI.Label(new Rect(18, 232, 300, 20),
                $"Init: {(_initCompleted ? "✔ Done" : "⏳ Wait")}    LastSave: {_lastSaveInfo}",
                green);

            GUI.Label(new Rect(18, 250, 300, 20),
                $"PlayerPos: {_player.Position:F2}", normal);
#endif
        }

        public void Dispose()
        {
            _hud.OnSaveClicked -= OnSave;
            _hud.OnMenuClicked -= OnMenu;
            _eventBus.Unsubscribe<GameSavedEvent>(OnGameSaved);
        }
    }

    // ── Proxy MonoBehaviour ───────────────────────────────────────────────

    internal class GameDebugProxy : MonoBehaviour
    {
        private GameController _controller;

        public static void Create(GameController controller)
        {
            var go = new GameObject("[DEBUG] GameDebugProxy");
            var proxy = go.AddComponent<GameDebugProxy>();
            proxy._controller = controller;
        }

        private void OnGUI() => _controller?.DrawDebugInfo();
        private void Update() => _controller?.DrawDebugInfo(); // DrawLine 需要在 Update
    }
}
