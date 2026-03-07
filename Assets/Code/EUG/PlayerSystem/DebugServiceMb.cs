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
using static UnityEngine.EventSystems.EventTrigger;

namespace Nobeta.EUG.FSM
{
    public class DebugServiceMb : MonoBehaviour
    {
        private Texture2D _myTex;
        private GUIStyle _myStyle;
        private Action DebugGizmos;
        private Action DebugGUI;
        public GizProxy Giz { get; private set; }
        public void Add(Action action) => DebugGUI += action;
        public void remove(Action action) => DebugGUI -= action;
        public void Clear() => DebugGUI = null;

        private void Awake()
        {
            Giz = new GizProxy(this);
            GUISettings();
        }
        public void DrawDebugText(string text)
        {

        }
        private void OnGUI()
        {
            if (DebugGUI != null)
            {
                if (!Application.isPlaying) return;
                _myStyle.normal.background = _myTex;
                GUILayout.BeginArea(new Rect(10, 10, 300, 400), _myStyle);
                DebugGUI?.Invoke();
                GUILayout.EndArea();
            }

        }
        private void OnDrawGizmos()
        {
            DebugGizmos?.Invoke();
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