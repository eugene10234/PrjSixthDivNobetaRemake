// ============================================================================
// PlayerController - 玩家控制器（完整事件驅動版）
// 純協調者，不包含任何實現邏輯
// ============================================================================


using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.FSM.States;
using System;
using UnityEngine;
using VContainer;
using static Nobeta.EUG.FSM.DebugServiceMb;
using static UnityEngine.EventSystems.EventTrigger;

namespace Nobeta.EUG.FSM
{
    public class DebugServiceMb : MonoBehaviour
    {
        private Texture2D _myTex;
        private GUIStyle _myStyle;

        // 儲存委託
        private Action DebugGizmos;
        private Action DebugGUI;
        // 修改點：改為 Lazy 模式，確保存取時一定有物件
        private GizProxy _giz;
        public GizProxy Giz => _giz ??= new GizProxy(this);

        private GUIProxy _gui;
        public GUIProxy GUI => _gui ??= new GUIProxy(this);
        //public GizProxy Giz { get; private set; }
        //public GUIProxy GUI { get; private set; }

        private void Awake()
        {
            //Giz = new GizProxy(this);
            //GUI = new GUIProxy(this);
            GUISettings();
        }
        public void DrawDebugText(string text)
        {

        }
        private void OnGUI()
        {
            if (DebugGUI == null || !Application.isPlaying) return;
            _myStyle.normal.background = _myTex;
            GUILayout.BeginArea(new Rect(10, 10, 300, 400), _myStyle);
            GUILayout.Label("=== DEBUG PANEL ===", GUILayout.ExpandWidth(true));
            DebugGUI?.Invoke();
            GUILayout.EndArea();

            // 如果你希望是手動 Add/Remove，就註解掉下面這一行
            //DebugGUI = null;
        }
        private void OnDrawGizmos()
        {
            if (DebugGizmos == null) return;
            DebugGizmos.Invoke();

            // 同理，Gizmos 也可以每幀清空，避免殘影
            //DebugGizmos = null;
        }
        // --- GUI 代理類 ---
        public class GUIProxy
        {
            private readonly DebugServiceMb _owner;
            public GUIProxy(DebugServiceMb owner) => _owner = owner;
            public void Add(Action action) => _owner.DebugGUI += action;
            public void Remove(Action action) => _owner.DebugGUI -= action;
            public void Clear() => _owner.DebugGUI = null;

            // 繪製一般文字
            public void Label(string text, Color? color = null)
            {
                Add(() =>
                {
                    //GUI.color = color ?? Color.white;
                    GUILayout.Label(text);
                });
            }

            // 追蹤變數 (例如: GUI.Watch("Speed", _speed))
            public void Watch(string label, object value, Color? color = null)
            {
                Add(() =>
                {
                    //GUILayout.color = color ?? Color.white;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{label}:", GUILayout.Width(100));
                    GUILayout.Label(value.ToString());
                    GUILayout.EndHorizontal();
                });
            }

            // 繪製一個按鈕 (雖然 Debug 較少用，但很有用)
            public void Button(string label, Action onClick)
            {
                Add(() =>
                {
                    if (GUILayout.Button(label))
                    {
                        onClick?.Invoke();
                    }
                });
            }
        }
        // 代理類：定義與 Gizmos 相同的函數名稱
        public class GizProxy
        {
            private readonly DebugServiceMb _owner;
            public GizProxy(DebugServiceMb owner) => _owner = owner;
            public void Add(Action action) => _owner.DebugGizmos += action;
            public void Remove(Action action) => _owner.DebugGizmos -= action;
            public void Clear() => _owner.DebugGizmos = null;
            public void DrawLine(Vector3 start, Vector3 end, Color? color = null)
            {
                _owner.Giz.Add(() =>
                {
                    Gizmos.color = color ?? Color.white;
                    Gizmos.DrawLine(start, end);
                });
            }

            public void DrawSphere(Vector3 center, float radius, Color? color = null)
            {
                _owner.Giz.Add(() =>
                {
                    Gizmos.color = color ?? Color.white;
                    Gizmos.DrawWireSphere(center, radius);
                });
            }
        }
        private void GUISettings()
        {
            _myTex = new Texture2D(1, 1);
            Color color = new Color(0, 0, 0, 0.5f); // 半透明黑色
            _myTex.SetPixel(0, 0, color); // Set your color here
            _myTex.Apply();
            _myStyle = new GUIStyle();
        }
    }

}