// ========================================================================
// Movement Service - 移動服務接口
// ========================================================================
using System;
using UnityEngine;
// ========================================================================
// Event Bus - 事件總線接口
// ========================================================================
namespace Nobeta.EUG.FSM.Interfaces
{
    /// <summary>
    /// 事件總線接口 - 全局事件系統
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// 訂閱事件
        /// </summary>
        void Subscribe<T>(Action<T> handler, int priority = 0) where T : struct;
        void Subscribe<T>(Action<T> handler) where T : struct;

        /// <summary>
        /// 取消訂閱事件
        /// </summary>
        void Unsubscribe<T>(Action<T> handler) where T : struct;

        /// <summary>
        /// 發布事件
        /// </summary>
        void Publish<T>(T gameEvent) where T : struct;

        /// <summary>
        /// 清除所有訂閱
        /// </summary>
        void Clear();
    }
}


