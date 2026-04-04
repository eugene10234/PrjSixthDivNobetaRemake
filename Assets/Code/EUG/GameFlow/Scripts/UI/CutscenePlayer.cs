using Cysharp.Threading.Tasks;
using Nobeta.EUG.GameFlow.Core.EventBus;
using Nobeta.EUG.GameFlow.Core.GameEvents;
using Nobeta.EUG.Interfaces;
using UnityEngine;

namespace Nobeta.EUG.GameFlow.UI
{
    /// <summary>
    /// 過場佔位元件。
    ///
    /// DrawDebugInfo() 同時做：
    ///   • OnGUI()          → Game View 顯示過場狀態 overlay
    ///   • Debug.DrawRay()  → Scene View 視覺化過場觸發點與方向
    ///
    /// 日後將 PlayAsync() 替換為 PlayableDirector.Play() 即可，介面不變。
    /// </summary>
    public class CutscenePlayer : MonoBehaviour
    {
        [SerializeField] private float _fakeDuration = 2.5f;

        private IEventBus _eventBus;
        private bool _isPlaying;
        private string _currentId = "";
        private float _elapsed;

        // ── Inject ──────────────────────────────────────────────────────

        public void Inject(IEventBus eventBus) => _eventBus = eventBus;

        // ── Public API ───────────────────────────────────────────────────

        public async UniTask PlayAsync(string cutsceneId)
        {
            _isPlaying = true;
            _currentId = cutsceneId;
            _elapsed = 0f;

            _eventBus?.Publish(new CutsceneStartedEvent(cutsceneId));

            // [PLACEHOLDER] 正式替換為 PlayableDirector.Play() / Cinemachine Sequence
            while (_elapsed < _fakeDuration)
            {
                _elapsed += Time.deltaTime;
                await UniTask.NextFrame();
            }

            _isPlaying = false;
            _eventBus?.Publish(new CutsceneCompletedEvent(cutsceneId));
        }

        // ── DrawDebugInfo ─────────────────────────────────────────────────

        private void Update() => DrawDebugInfo();
        private void OnGUI() => DrawDebugInfoGUI();

        /// <summary>
        /// Scene View：Debug.DrawRay で過場觸發點 + 播放方向可視化。
        /// </summary>
        private void DrawDebugInfo()
        {
            if (!_isPlaying) return;

            float progress = Mathf.Clamp01(_elapsed / _fakeDuration);

            // 觸發點位置（綠色 = 起點）
            Debug.DrawRay(
                transform.position,
                Vector3.up * 2f,
                Color.green
            );

            // 播放進度（黃色 ray 長度對應進度 0→1）
            Debug.DrawRay(
                transform.position + Vector3.up * 2.2f,
                Vector3.right * (progress * 3f),
                Color.yellow
            );

            // 過場範圍邊界（白色框）
            DrawDebugBox(transform.position, new Vector3(4f, 3f, 0.1f), Color.white);
        }

        /// <summary>
        /// Game View：OnGUI overlay 顯示過場 ID 與進度條。
        /// </summary>
        private void DrawDebugInfoGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!_isPlaying) return;

            float progress = Mathf.Clamp01(_elapsed / _fakeDuration);

            // 半透明背景
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.DrawTexture(new Rect(10, 10, 320, 70), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // 標題
            GUI.Label(new Rect(18, 14, 300, 24),
                $"🎬 [CUTSCENE DEBUG]  id: '{_currentId}'",
                new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold });

            // 進度文字
            GUI.Label(new Rect(18, 36, 300, 20),
                $"Progress: {progress:P0}  ({_elapsed:F1}s / {_fakeDuration:F1}s)",
                new GUIStyle(GUI.skin.label) { fontSize = 11 });

            // 進度條背景
            GUI.color = new Color(1f, 1f, 1f, 0.3f);
            GUI.DrawTexture(new Rect(18, 58, 290, 12), Texture2D.whiteTexture);

            // 進度條填充
            GUI.color = Color.green;
            GUI.DrawTexture(new Rect(18, 58, 290f * progress, 12), Texture2D.whiteTexture);

            GUI.color = Color.white;
#endif
        }

        // ── Helpers ──────────────────────────────────────────────────────

        private static void DrawDebugBox(Vector3 center, Vector3 size, Color color)
        {
            Vector3 h = size * 0.5f;
            Vector3 tl = center + new Vector3(-h.x, h.y, 0);
            Vector3 tr = center + new Vector3(h.x, h.y, 0);
            Vector3 bl = center + new Vector3(-h.x, -h.y, 0);
            Vector3 br = center + new Vector3(h.x, -h.y, 0);

            Debug.DrawLine(tl, tr, color);
            Debug.DrawLine(tr, br, color);
            Debug.DrawLine(br, bl, color);
            Debug.DrawLine(bl, tl, color);
        }
    }
}
