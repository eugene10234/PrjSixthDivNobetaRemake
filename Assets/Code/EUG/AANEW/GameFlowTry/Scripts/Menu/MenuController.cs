using Nobeta.EUG.GameFlowTry.Core.EventBus;
using Nobeta.EUG.GameFlowTry.Core.GameEvents;
using Nobeta.EUG.GameFlowTry.Core.Save;
using UnityEngine;
using VContainer.Unity;

namespace Nobeta.EUG.GameFlowTry.Menu
{
    /// <summary>
    /// MenuScene 邏輯控制器（純 C# EntryPoint）。
    /// DrawDebugInfo() 在 Game View 顯示選單狀態 overlay。
    /// </summary>
    public class MenuController : IStartable
    {
        private readonly IEventBus2 _eventBus;
        private readonly ISaveSystem _saveSystem;
        private readonly MenuView _view;

        // debug state
        private bool _hasSave;
        private string _lastAction = "none";

        public MenuController(IEventBus2 eventBus, ISaveSystem saveSystem, MenuView view)
        {
            _eventBus = eventBus;
            _saveSystem = saveSystem;
            _view = view;
        }

        public void Start()
        {
            _hasSave = _saveSystem.HasSave;
            _view.SetContinueInteractable(_hasSave);
            _view.ShowStatus(_hasSave ? "找到存檔，可以繼續遊戲。" : "尚無存檔。");

            _view.OnNewGameClicked += OnNewGame;
            _view.OnContinueClicked += OnContinue;

            // MenuController 是純 C#，掛 DrawDebugInfo 到一個 MonoBehaviour Proxy
            MenuDebugProxy.Create(this);
        }

        private void OnNewGame()
        {
            _lastAction = "New Game → GameScene1";
            _eventBus.Publish(new NewGameRequestedEvent());
        }

        private void OnContinue()
        {
            var save = _saveSystem.Load();
            _lastAction = save != null
                ? $"Continue → {save.SceneName} @ {save.PlayerPosition}"
                : "Continue → no save, fallback NewGame";
            _eventBus.Publish(new ContinueGameRequestedEvent());
        }

        // ── DrawDebugInfo ─────────────────────────────────────────────────

        /// <summary>由 MenuDebugProxy 每幀呼叫。</summary>
        public void DrawDebugInfo()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // ── OnGUI overlay ──────────────────────────────────────────
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(10, 10, 300, 80), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(18, 14, 280, 22),
                "📋 [MENU DEBUG]",
                new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold });

            GUI.Label(new Rect(18, 36, 280, 20),
                $"HasSave: {_hasSave}",
                new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = _hasSave ? Color.green : Color.red } });

            GUI.Label(new Rect(18, 54, 280, 20),
                $"Last Action: {_lastAction}",
                new GUIStyle(GUI.skin.label) { fontSize = 11 });

            GUI.Label(new Rect(18, 70, 280, 20),
                _hasSave ? $"Save: {_saveSystem.Load()?.SceneName} @ {_saveSystem.Load()?.PlayerPosition}" : "—",
                new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = Color.cyan } });
#endif
        }
    }

    // ── Proxy MonoBehaviour（自動建立，無需手動掛） ───────────────────────

    /// <summary>
    /// 純 C# 的 MenuController 無法呼叫 OnGUI，
    /// 此 Proxy 由 MenuController 自動生成並橋接。
    /// </summary>
    internal class MenuDebugProxy : MonoBehaviour
    {
        private MenuController _controller;

        public static void Create(MenuController controller)
        {
            var go = new GameObject("[DEBUG] MenuDebugProxy");
            var proxy = go.AddComponent<MenuDebugProxy>();
            proxy._controller = controller;
        }

        private void OnGUI() => _controller?.DrawDebugInfo();
    }
}
