using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
// ============================================================================
// BulletVisualService v2
//
// 回答你的問題：「傷害 Strategy 需要另一個 BulletVisualService 嗎？」
//
//   不需要。
//   做法：BulletHitEvent 帶 HitType（Single / Aoe / Dot）
//   VisualService 依 HitType switch 播放對應特效。
//   Strategy 只負責傷害邏輯，完全不碰 VFX。
//   VFX 層只訂閱 EventBus 事件，不需要知道 Strategy 的存在。
//
// ============================================================================
namespace Nobeta.EUG.BulletSystemV2.OOP.UnityBridge
{
    public class BulletVisualPoolData
    {
        public readonly BulletVFXConfig config;
        public readonly Transform parent;
        public Transform rootTransform; // 用來當作 Hierarchy 中的父節點（選配）

        public Stack<GameObject> BulletVisual = new Stack<GameObject>();
        public Stack<GameObject> MuzzleEffect = new Stack<GameObject>();
        public Stack<GameObject> TrailRender = new Stack<GameObject>();
        public Stack<GameObject> HitEffect = new Stack<GameObject>();
        
        public BulletVisualPoolData(BulletVFXConfig cfg, Transform visualRoot)
        {
            config = cfg;
            parent = visualRoot;
            BulletVisual = new Stack<GameObject>();
            MuzzleEffect = new Stack<GameObject>();
            TrailRender = new Stack<GameObject>();
            HitEffect = new Stack<GameObject>();
            rootTransform = new GameObject($"{cfg.Type}_VFXPool").transform;
            rootTransform.SetParent(visualRoot);
        }
        public void PrewarmPools(int count)
        {

            // 預熱子彈視覺
            if (config.BulletVisualPrefab != null)
            {
                for (int i = 0; i < count; i++)
                {
                    var visual = Object.Instantiate(config.BulletVisualPrefab, rootTransform);
                    visual.SetActive(false);
                    BulletVisual.Push(visual);
                }
            }

            // 預熱槍口閃光
            if (config.MuzzleFlashPrefab != null)
            {
                for (int i = 0; i < count / 2; i++)
                {
                    var muzzle = Object.Instantiate(config.MuzzleFlashPrefab, rootTransform);
                    muzzle.SetActive(false);
                    MuzzleEffect.Push(muzzle);
                }
            }

            // 預熱拖尾
            if (config.TrailRendererPrefab != null)
            {
                for (int i = 0; i < count; i++)
                {
                    var trail = Object.Instantiate(config.TrailRendererPrefab, rootTransform);
                    trail.SetActive(false);
                    TrailRender.Push(trail);
                }
            }

            // 預熱擊中特效
            if (config.HitEffectPrefab != null)
            {
                for (int i = 0; i < count; i++)
                {
                    var hit = Object.Instantiate(config.HitEffectPrefab, rootTransform);
                    hit.SetActive(false);
                    HitEffect.Push(hit);
                }
            }
        }
    }
}