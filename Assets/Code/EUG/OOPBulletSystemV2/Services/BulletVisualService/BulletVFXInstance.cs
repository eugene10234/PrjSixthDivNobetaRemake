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
    /// <summary>
    /// 子彈 VFX 實例（包含所有特效引用）
    /// </summary>
    internal class BulletVFXInstance
    {
        public GameObject bulletVisual;  // 子彈本體
        public GameObject trail;         // 拖尾粒子
    }
}