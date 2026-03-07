// ========================================================================
// Ground Detection Service - 地面檢測服務接口
// ========================================================================
using Nobeta.EUG.FSM.Core;
using System;
using UnityEngine;

namespace Nobeta.EUG.FSM.Interfaces
{

    /// <summary>
    /// 地面檢測服務接口
    /// </summary>
    public interface IDetectionService
    {
        /// <summary>
        /// 是否在地面上
        /// </summary>
        bool IsGrounded { get; }

        /// <summary>
        /// 最後一次著地時間
        /// </summary>
        float LastGroundedTime { get; }

        /// <summary>
        /// 更新地面檢測
        /// </summary>
        void UpdateGlobalDetection(StateContext ctx);

        bool IsMoving { get; }
        bool IsTurning { get; }
    }


}


