using UnityEngine;
using UnityEngine.Animations.Rigging; // 記得引用 Rigging 命名空間

public class AdvancedRigHeadController : MonoBehaviour
{
    [Header("相關元件")]
    public Rig rig;                   // 你的 Rig 元件
    public Transform rigTarget;      // Rig 的目標點 (Target)
    public Transform headBone;       // 頭部骨骼
    public Transform target;         // 追蹤目標

    [Header("角度限制")]
    public float minYaw = -90f;
    public float maxYaw = 90f;
    public float minPitch = -50f;
    public float maxPitch = 50f;

    [Header("平滑與權重設定")]
    public float smoothTime = 0.15f;
    public float weightSpeed = 2f;    // 權重增減的速度
    public float targetDistance = 3f;

    private float currentYaw;
    private float currentPitch;
    private float yawVelocity;
    private float pitchVelocity;
    private float customMappedValue; // 你要求的 0.36 映射值

    void Update()
    {
        bool hasTarget = target != null;
        float targetWeight = hasTarget ? 1f : 0f;

        // 1. 處理 Rig 權重的平滑切換 (Fade In/Out)
        if (rig != null)
        {
            rig.weight = Mathf.MoveTowards(rig.weight, targetWeight, weightSpeed * Time.deltaTime);
        }

        // 2. 計算目標角度 (如果沒目標，則回正 0,0)
        float targetYaw = 0f;
        float targetPitch = 0f;

        if (hasTarget)
        {
            Vector3 localTarget = headBone.parent.InverseTransformPoint(target.position);
            targetYaw = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
            float horizontalDist = new Vector2(localTarget.x, localTarget.z).magnitude;
            targetPitch = Mathf.Atan2(localTarget.y, horizontalDist) * Mathf.Rad2Deg;

            // 限制角度 (避免斷頭)
            targetYaw = Mathf.Clamp(targetYaw, minYaw, maxYaw);
            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
        }

        // 3. 使用 SmoothDamp 確保不經過 180 度大跳變
        currentYaw = Mathf.SmoothDamp(currentYaw, targetYaw, ref yawVelocity, smoothTime);
        currentPitch = Mathf.SmoothDamp(currentPitch, targetPitch, ref pitchVelocity, smoothTime);

        // 4. 【執行你要求的數值映射邏輯】
        // 0~180 -> 0.36 | 0~-180 -> 0.36 ~ -0.36
        customMappedValue = CalculateCustomValue(currentYaw);

        // 5. 更新 RigTarget 位置
        Quaternion lookRotation = Quaternion.Euler(-currentPitch, currentYaw, 0f);
        Vector3 localPos = lookRotation * Vector3.forward * targetDistance;

        // 額外應用：你可以把映射值加在 X 位移上作為微調範例
        localPos.x += customMappedValue;

        rigTarget.position = headBone.parent.TransformPoint(localPos);
    }

    private float CalculateCustomValue(float yaw)
    {
        if (yaw >= 0) return 0.36f;

        // 0 到 -180 的線性映射
        float t = yaw / -180f; // 0 時為 0, -180 時為 1
        return Mathf.Lerp(0.36f, -0.36f, t);
    }
}