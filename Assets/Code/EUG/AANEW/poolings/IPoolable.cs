using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Nobeta.EUG.Core.Pooling
{
    // ========================================================================
    // IPoolable - 可池化對象接口
    // ========================================================================

    /// <summary>
    /// 可池化對象接口
    /// 實現此接口以自定義對象的重置邏輯
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 從池中取出時調用
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// 歸還到池中時調用
        /// </summary>
        void OnDespawn();
    
    }
}