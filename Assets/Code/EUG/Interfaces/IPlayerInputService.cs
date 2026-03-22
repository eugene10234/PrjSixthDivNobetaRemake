// ========================================================================
// Input Service - 輸入服務接口
// ========================================================================
using System;
using UnityEngine;

namespace Nobeta.EUG.Interfaces
{
    /// <summary>
    /// 輸入服務接口 - 提供輸入狀態和事件
    /// 你已有的 PlayerInputService 已經很完美了！
    /// 這裡定義接口以便依賴注入
    /// </summary>
    public interface IPlayerInputService
    {
        /// <summary>
        /// 移動輸入（歸一化）
        /// </summary>
        Vector2 MoveInput { get; }
        Vector2 MousePosOffset { get; }
        /// <summary>
        /// 是否按下跳躍
        /// </summary>
        bool IsJumpPressed { get; }

        /// <summary>
        /// 是否按下奔跑
        /// </summary>
        bool IsRunPressed { get; }
        bool IsAimPressed { get; }
        bool IsFirePressed { get; }
        bool IsChantPressed { get; }
        int PressedNumSwitch { get; }


        /// <summary>
        /// 移動事件
        /// </summary>
        event Action OnMove;

        /// <summary>
        /// 停止移動事件
        /// </summary>
        event Action OnStop;

        /// <summary>
        /// 奔跑事件
        /// </summary>
        event Action OnRun;

        /// <summary>
        /// 跳躍事件
        /// </summary>
        event Action OnJump;
        /// <summary>
        /// 閃避事件
        /// </summary>
        event Action OnDodge;
        /// <summary>
        /// 瞄準事件
        /// </summary>
        event Action OnAimStart;
        /// <summary>
        /// 瞄準事件
        /// </summary>
        event Action OnAimEnd;
        /// <summary>
        /// 發射事件
        /// </summary>
        event Action OnFireStart;
        event Action OnFireEnd;
        event Action OnAttack;
        event Action OnChant;
        event Action OnNumSwitch;
        event Action OnCusorSwitchStart;
        event Action OnCusorSwitchEnd;
    }


}


