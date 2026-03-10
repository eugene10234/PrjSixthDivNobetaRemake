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
    // Pool Extensions（擴展方法）
    // ========================================================================

    public static class PoolExtensions
    {
        /// <summary>
        /// 使用 UniTask 實現延遲歸還
        /// </summary>
        public static async UniTaskVoid TaskReturnToPoolAfter<T>(this T obj, Pool<T> pool, float delay)
            where T : Component
        {
            // 取得該物件的 CancellationToken，確保物件如果被 Destroy，非同步任務會自動停止
            var ct = obj.GetCancellationTokenOnDestroy();

            try
            {
                // 延遲 (毫秒單位，所以秒數要 * 1000)
                await UniTask.Delay((int)(delay * 1000), cancellationToken: ct);

                // 檢查物件是否還在（預防意外銷毀）
                if (obj != null)
                {
                    pool.Return(obj);
                }
            }
            catch (System.OperationCanceledException)
            {
                // 任務被取消時（例如物件被 Destroy 了），不做任何事
            }
        }
    }
}