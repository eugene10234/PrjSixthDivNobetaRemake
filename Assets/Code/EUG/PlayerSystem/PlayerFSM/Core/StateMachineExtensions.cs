using Nobeta.EUG.FSM.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nobeta.EUG.FSM.Core
{
    public static class StateMachineExtensions
    {
        /// <summary>
        /// 取得歷史記錄中最後一個屬於 GroundedState 的類型
        /// </summary>
        public static Type GetLastGroundedState(this StateMachine stateMachine)
        {
            if (stateMachine == null) throw new ArgumentNullException(nameof(stateMachine));

            // 這裡假設 GetStateHistory() 是 StateMachine 的 public 方法
            return stateMachine.GetStateHistory()
                .LastOrDefault(type => typeof(GroundedState).IsAssignableFrom(type));
        }
    }
}
