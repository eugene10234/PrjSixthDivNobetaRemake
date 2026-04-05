using Cysharp.Threading.Tasks;
using Nobeta.EUG.BulletSystemV2.OOP;
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
namespace Nobeta.EUG.BulletSystemV2ExampleBad.OOP.UnityBridge
{
    [System.Serializable]
    public class BulletVFXConfigExampleBad
    {
        public BulletType Type;

        [Header("飛行視覺")]
        public GameObject BulletVisualPrefab;

        [Header("命中特效 (依 HitType)")]
        public GameObject HitEffectSingle;   // 單體命中
        public GameObject HitEffectAoe;      // 範圍爆炸（半徑會 Scale 到 AoeRadius）
        public GameObject HitEffectDot;      // DoT 開始（燃燒 / 中毒貼圖）

        [Header("槍口閃光")]
        public GameObject MuzzleFlashPrefab;
    }  
}