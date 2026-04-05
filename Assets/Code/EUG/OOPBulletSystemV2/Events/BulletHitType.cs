using UnityEngine;
// ============================================================================
// BulletEvents v2  ── BulletHitEvent 加入 HitType，讓 VFX 知道要播哪種特效
// ============================================================================
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    /// <summary>
    /// 命中類型，BulletVisualService 依此決定播放哪種特效
    /// </summary>
    public enum BulletHitType
    {
        Single,  // 單體命中
        Aoe,     // 範圍爆炸
        Dot,     // DoT 開始（播上毒/燃燒貼圖）
    } 
}