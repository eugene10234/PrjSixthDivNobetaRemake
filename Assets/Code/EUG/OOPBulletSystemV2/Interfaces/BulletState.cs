using UnityEngine;
// ════════════════════════════════════════════════════════════════════════
// BulletState  ── Bullet 的可變狀態，strategy 直接讀寫
// ════════════════════════════════════════════════════════════════════════

namespace Nobeta.EUG.BulletSystemV2.OOP
{
    /// <summary>
    /// 子彈的完整可變狀態。
    /// 用 class（reference type）讓 strategy 可以直接修改，不需要 ref 傳遞。
    /// </summary>
    public class BulletState
    {
        public int Id;
        public Vector3 Position;
        public Vector3 Velocity;
        public Vector3 Direction;
        public Quaternion Rotation;
        public float Speed;
        public int Damage;
        public BulletType Type;
        public float Lifetime;
        public float ColliderRadius;
        public bool IsActive;

        // Movement Strategy 用的額外狀態
        public float ElapsedTime;   // 累計時間（拋物線 / 導彈用）
        public Vector3 LaunchPosition; // 初始位置（拋物線用）
        public Transform Target;        // 追蹤目標（導彈用）
    }
}
