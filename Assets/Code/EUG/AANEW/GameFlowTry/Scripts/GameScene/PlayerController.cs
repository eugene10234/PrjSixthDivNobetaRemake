using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nobeta.EUG.GameFlowTry.GameScene
{
    /// <summary>
    /// 玩家移動，DrawDebugInfo() 視覺化位置、速度向量、Spawn 點。
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private CharacterController _cc;

        private Vector3 _spawnPoint;
        private Vector3 _velocity;

        private void Update()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            _velocity = new Vector3(h, 0, v).normalized * _moveSpeed;
            if (_velocity.magnitude > 0.1f)
                _cc.Move(_velocity * Time.deltaTime);

            DrawDebugInfo();
        }
        // ── DrawDebugInfo ─────────────────────────────────────────────────

        private void DrawDebugInfo()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Scene View：速度向量（青色）
            Debug.DrawRay(transform.position, _velocity.normalized * 1.5f, Color.cyan);

            // Scene View：Spawn 點 → 當前位置（黃色）
            Debug.DrawLine(_spawnPoint, transform.position, Color.yellow);

            // Scene View：Spawn 點標記（紅色十字）
            Debug.DrawRay(_spawnPoint, Vector3.up * 0.5f, Color.red);
            Debug.DrawRay(_spawnPoint, Vector3.right * 0.5f, Color.red);
            Debug.DrawRay(_spawnPoint, Vector3.forward * 0.5f, Color.red);
#endif
        }

        private void OnGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(10, 100, 280, 60), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(18, 104, 260, 22),
                "🧍 [PLAYER DEBUG]",
                new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold });

            GUI.Label(new Rect(18, 124, 260, 20),
                $"Pos: {transform.position:F2}",
                new GUIStyle(GUI.skin.label) { fontSize = 11 });

            GUI.Label(new Rect(18, 142, 260, 20),
                $"Spawn: {_spawnPoint:F2}   Speed: {_velocity.magnitude:F1}",
                new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = Color.yellow } });
#endif
        }

    }
}
