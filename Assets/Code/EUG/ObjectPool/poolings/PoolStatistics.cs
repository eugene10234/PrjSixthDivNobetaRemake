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
    /// <summary>
    /// 池統計信息
    /// </summary>
    public struct PoolStatistics
    {
        public int ActiveCount;
        public int PooledCount;
        public int TotalCreated;
        public int TotalGets;
        public int TotalReturns;
        public float HitRate;
    }
}