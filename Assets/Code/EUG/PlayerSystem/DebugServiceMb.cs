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
        public void Add(Action action) => DebugGUI += action;
        public void remove(Action action) => DebugGUI -= action;
        public void Clear() => DebugGUI = null;

        private Action DebugGUI;
        private void OnGUI()
        {
            DebugGUI?.Invoke();

        }
       
    }

}