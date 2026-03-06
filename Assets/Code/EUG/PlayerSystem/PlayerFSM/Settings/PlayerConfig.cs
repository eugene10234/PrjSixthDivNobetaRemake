
// ========================================================================
// PlayerConfig - 配置 ScriptableObject
// ========================================================================

using UnityEngine;
namespace Nobeta.EUG.FSM.Settings
{
    /// <summary>
    /// 玩家配置
    /// 所有可調整的參數
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "SO/Create Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("移動設置")]
        public float walkSpeed = 6f;
        public float runSpeed = 10f;
        public float rotationSpeed = 50f;
        public float acceleration = 10f;

        [Header("跳躍設置")]
        public float jumpHeight = 2f;
        public float gravity = -15f;
        public float coyoteTime = 0.2f;

        [Header("閃避設置")]
        public float dodgeDistance = 6f;
        public float dodgeDuration = 0.5f;
        public float dodgeCooldown = 1f;
        public int maxDodgeCharges = 2;

        [Header("魔法設置")]
        public float maxChargeTime = 2f;
        public float castDuration = 0.5f;

        [Header("戰鬥設置")]
        public float meleeAttackDuration = 0.6f;
        public float hitStunDuration = 0.4f;
        public float comboWindow = 1f;
    }
}