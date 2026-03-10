// ============================================================================
// Professional ObjectPool - 專業級對象池
// 零 GC、高性能、功能完整的對象池系統
// ============================================================================

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Nobeta.EUG.Core.Pooling
{
    // ========================================================================
    // Configuration（配置）
    // ========================================================================

    /// <summary>
    /// 池配置
    /// </summary>
    public class PoolConfig
    {
        /// <summary>
        /// 初始容量
        /// </summary>
        public int InitialCapacity { get; set; } = 10;

        /// <summary>
        /// 最大容量（0 = 無限制）
        /// </summary>
        public int MaxCapacity { get; set; } = 0;

        /// <summary>
        /// 預熱數量
        /// </summary>
        public int PrewarmCount { get; set; } = 0;

        /// <summary>
        /// 池耗盡時是否拋出異常
        /// </summary>
        public bool ThrowOnExhausted { get; set; } = false;

        /// <summary>
        /// 池耗盡時是否回收最老的對象
        /// </summary>
        public bool RecycleOldestOnExhausted { get; set; } = true;

        /// <summary>
        /// 是否在場景切換時保留
        /// </summary>
        public bool DontDestroyOnLoad { get; set; } = true;
    }
}