// ============================================================================
// MovementService - 移動服務完整實現
// 處理所有移動相關的計算和物理應用
// ============================================================================

using System;
using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.Interfaces;
using UnityEngine;
using VContainer;

namespace Nobeta.EUG.FSM.Services
{
    /// <summary>
    /// 移動服務實現
    /// 職責：
    /// - 計算移動速度
    /// - 應用重力
    /// - 處理旋轉
    /// - 執行物理移動
    /// 
    /// 不負責：
    /// - 輸入處理
    /// - 狀態邏輯
    /// - 動畫播放
    /// </summary>
    public class MovementService : IMovementService
    {
        private readonly CharacterController controller;
        private readonly Transform transform;
        private readonly ITimeService timeService;
        private readonly ICameraService _cameraService;
        private Vector3 velocity;

        // 屬性
        public Vector3 CurrentVelocity => velocity;

        public float CurrentHorizontalSpeed =>
            new Vector3(velocity.x, 0, velocity.z).magnitude;
        DebugServiceMb _debugServiceMb;
        private float text1;
        private float text2;
        private float text3;

        // 構造函數注入
        [Inject]
        public MovementService(
            DebugServiceMb debugServiceMb,
            CharacterController controller,
            Transform transform,
            ICameraService cameraService,
            ITimeService timeService)
        {
            _debugServiceMb = debugServiceMb;
            this.controller = controller;
            this.transform = transform;

            this.timeService = timeService;
            _cameraService = cameraService;
            velocity = Vector3.zero;
            //_debugServiceMb.GUI.Add(DrawDebugInfo);
        }

        private void DrawDebugInfo()
        {
            GUILayout.Box("=== MovementService ===");
            GUILayout.Label($"Camera Influence: {text1}");
            GUILayout.Label($"Dynamic Speed: {text2}");
            GUILayout.Label($"angleBoost Dynamic Speed: {text3}");
        }
        #region Ground Movement

        public void ApplyGroundMovement(
            Vector3 direction,
            float targetSpeed,
            float acceleration)
        {
            if (direction.magnitude > 0.1f)
            {
                // 計算目標速度向量
                Vector3 targetVelocity = direction.normalized * targetSpeed;

                // 平滑插值到目標速度
                velocity.x = Mathf.Lerp(
                    velocity.x,
                    targetVelocity.x,
                    timeService.DeltaTime * acceleration
                );

                velocity.z = Mathf.Lerp(
                    velocity.z,
                    targetVelocity.z,
                    timeService.DeltaTime * acceleration
                );
            }
            else
            {
                // 沒有輸入時減速停止
                StopMovement(acceleration);
            }
        }

        #endregion

        #region Air Movement

        public void ApplyAirMovement(
            float previousSpeed,
            Vector3 previousDirection,
            Vector3 direction,
            float targetSpeed,
            float airControlFactor)
        {
            if (direction.magnitude > 0.1f)
            {
                Vector3 targetVelocity = direction.normalized * targetSpeed;
                float offsetSpeed = Vector3.Dot(previousDirection, direction) * (previousSpeed- targetSpeed);
                Vector3 offsetVelocity = direction.normalized * offsetSpeed;
                Vector3 offsettedTargetVelocity = targetVelocity + offsetVelocity;
                // 空中控制力較弱
                float airAcceleration = 5f * airControlFactor;

                velocity.x = Mathf.Lerp(
                    velocity.x,
                    offsettedTargetVelocity.x,
                    timeService.DeltaTime * airAcceleration
                );

                velocity.z = Mathf.Lerp(
                    velocity.z,
                    offsettedTargetVelocity.z,
                    timeService.DeltaTime * airAcceleration
                );

            }
        }

        #endregion

        #region Gravity and Jump

        public void ApplyGravity(float gravity)
        {
            velocity.y += gravity * timeService.DeltaTime;

            // 限制最大下落速度（可選）
            velocity.y = Mathf.Max(velocity.y, -50f);
        }

        public void ApplyJump(float jumpHeight, float gravity)
        {
            // 根據跳躍高度計算初速度
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        #endregion

        #region Dodge Movement

        public void ApplyDodge(Vector3 direction, float distance, float duration)
        {
            if (direction.magnitude < 0.1f) return;

            // 計算閃避速度
            float dodgeSpeed = distance / duration;

            velocity.x = direction.normalized.x * dodgeSpeed;
            velocity.z = direction.normalized.z * dodgeSpeed;
        }

        #endregion

        #region Rotation


        /// <summary>
        /// 平滑旋轉到目標方向(帶相機影響的動態速度)
        /// </summary>
        public void RotateTowards(Vector3 direction, float baseRotationSpeed)
        {
            if (direction.sqrMagnitude < 0.01f) return;

            // 計算目標旋轉
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 計算當前角度差
            float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

            // 方案1: 根據相機旋轉速度動態調整
            // 當玩家快速轉動相機時,角色也快速跟上
            float cameraInfluence = Mathf.Abs(_cameraService.GetHorizonOffset);
            float dynamicSpeed = baseRotationSpeed * (1f + cameraInfluence * 1f);
            //Debug.Log($"Camera Influence: {cameraInfluence}, Dynamic Speed: {dynamicSpeed}");
            //text1 = cameraInfluence;
            //text2 = dynamicSpeed;
            // 方案2(可選): 根據角度差額外加速
            // 當需要轉很大角度時,加快旋轉避免轉太慢
            float angleBoost = Mathf.Clamp01(angleDifference / 120f) * 1.2f;
            if(angleDifference>45f) dynamicSpeed *= (1f + angleBoost);
            //Debug.Log($"angleBoost Dynamic Speed: {dynamicSpeed}");
            //text3 = angleDifference;
            // 平滑旋轉
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                timeService.DeltaTime * dynamicSpeed
            );
        }
        /// <summary>
        /// 即時旋轉(不平滑,用於射擊等需要精確對準的情況)
        /// </summary>
        public void RotateTowardsImmediate(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.LookRotation(direction);
        }

        /// <summary>
        /// 固定速度平滑旋轉(不受相機影響)
        /// </summary>
        public void RotateTowardsSteady(Vector3 direction, float rotationSpeed)
        {
            if (direction.sqrMagnitude < 0.01f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                timeService.DeltaTime * rotationSpeed
            );
        }
        #endregion

        #region Utility Methods

        public void StopMovement(float deceleration)
        {
            // 水平方向減速
            velocity.x = Mathf.Lerp(
                velocity.x,
                0,
                timeService.DeltaTime * deceleration
            );

            velocity.z = Mathf.Lerp(
                velocity.z,
                0,
                timeService.DeltaTime * deceleration
            );
        }
        public void ReduceMovement(float deceleration,float reducevalue)
        {
            // 水平方向減速
            velocity.x = Mathf.Lerp(
                velocity.x,
                velocity.x* reducevalue,
                timeService.DeltaTime * deceleration
            );

            velocity.z = Mathf.Lerp(
                velocity.z,
                velocity.z* reducevalue,
                timeService.DeltaTime * deceleration
            );
        }
        public void ResetVelocity()
        {
            velocity = Vector3.zero;
        }
        public void remainVelocity(float offset)
        {
            velocity *= offset;
        }

        public void SetVerticalVelocity(float verticalVelocity)
        {
            velocity.y = verticalVelocity;
        }

        #endregion

        #region Execute Movement

        public void ExecuteMovement()
        {
            // 應用速度到 CharacterController
            controller.Move(velocity * timeService.DeltaTime);
        }

        #endregion
    }
}