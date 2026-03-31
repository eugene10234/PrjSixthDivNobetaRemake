using UnityEngine;

namespace Nobeta.EUG.GameFlowTry.GameScene
{
    public class SceneSpawnPoint : MonoBehaviour
    {
        private Vector3 _position;
        private Quaternion _rotation;
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        private void Awake()
        {
            SetGround();
            Position = transform.position;
            Rotation = transform.rotation;            
        }
        public void SetGround()
        {
            // 1. 取得 Layer 的索引並轉換為 Bitmask
            // 這裡會同時偵測 "Ground" 或 "Obstacle" 層
            int layerMask = LayerMask.GetMask("Ground", "Obstacle");

            // 2. 設定射線參數
            float rayOffset = 0.5f; // 從中心往上提一點，避免穿模
            float maxDistance = 20f; // 往下偵測的距離
            Vector3 rayStart = transform.position + Vector3.up * rayOffset;

            // 3. 執行 Raycast
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, maxDistance, layerMask))
            {
                // 移動到碰撞點
                transform.position = hit.point;

                // 偵錯用：在 Scene 視窗畫出一條紅線
                Debug.DrawLine(rayStart, hit.point, Color.green, 2.0f);
            }
            else
            {
                // 偵錯用：如果沒打到，畫一條紅線表示範圍
                Debug.DrawRay(rayStart, Vector3.down * maxDistance, Color.red, 2.0f);
                Debug.LogWarning($"{transform.name} 下方沒有找到 Ground 或 Obstacle 層的物體。");
            }
        }
    }
}