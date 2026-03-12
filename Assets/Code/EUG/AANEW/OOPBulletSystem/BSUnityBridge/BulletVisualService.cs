using System.Collections.Generic;
using Nobeta.EUG.BulletSystem.OOP;
//using Nobeta.EUG.Core.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;
// ========================================================================
// VisualService (視覺服務)
// ========================================================================
namespace Nobeta.EUG.BulletSystem.OOP.UnityBridge
{
    /// <summary>
    /// 視覺服務（管理子彈視覺效果）
    /// </summary>
    public class BulletVisualService
    {
        private readonly GameObject[] visualPrefabs;
        private readonly Stack<GameObject>[] visualPools;
        private readonly Transform visualContainer;

        // 活動視覺：Bullet → GameObject
        private readonly Dictionary<Bullet, GameObject> activeVisuals;

        public BulletVisualService(GameObject[] prefabs, Transform container)
        {
            visualPrefabs = prefabs;
            visualContainer = container;

            activeVisuals = new Dictionary<Bullet, GameObject>();

            // 初始化每種類型的池
            visualPools = new Stack<GameObject>[prefabs.Length];
            for (int i = 0; i < prefabs.Length; i++)
            {
                visualPools[i] = new Stack<GameObject>();
                PrewarmVisuals(i, 20);
            }
        }

        private void PrewarmVisuals(int type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var visual = Object.Instantiate(visualPrefabs[type], visualContainer);
                visual.SetActive(false);
                visualPools[type].Push(visual);
            }
        }

        /// <summary>
        /// 為子彈分配視覺
        /// </summary>
        public void AssignVisual(Bullet bullet)
        {
            int type = (int)bullet.Type;

            // 從池獲取
            GameObject visual;
            if (visualPools[type].Count > 0)
            {
                visual = visualPools[type].Pop();
            }
            else
            {
                visual = Object.Instantiate(visualPrefabs[type], visualContainer);
            }

            // 激活並設置位置
            visual.SetActive(true);
            visual.transform.position = bullet.Position;
            visual.transform.rotation = bullet.Rotation;

            // 記錄映射
            activeVisuals[bullet] = visual;
        }

        /// <summary>
        /// 回收視覺
        /// </summary>
        public void RecycleVisual(Bullet bullet)
        {
            if (!activeVisuals.TryGetValue(bullet, out var visual))
                return;

            // 停用
            visual.SetActive(false);

            // 歸還到池
            int type = (int)bullet.Type;
            visualPools[type].Push(visual);

            // 移除映射
            activeVisuals.Remove(bullet);
        }

        /// <summary>
        /// 同步視覺到邏輯
        /// </summary>
        public void SyncVisualsToLogic(IReadOnlyList<Bullet> bullets)
        {
            foreach (var bullet in bullets)
            {
                if (!activeVisuals.TryGetValue(bullet, out var visual))
                {
                    // 新子彈，分配視覺
                    AssignVisual(bullet);
                    continue;
                }

                // 同步位置和旋轉
                visual.transform.position = bullet.Position;
                visual.transform.rotation = bullet.Rotation;
            }
        }
    }
}


