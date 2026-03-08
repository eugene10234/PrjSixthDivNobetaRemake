// ============================================================================
// MovementService - 移動服務完整實現
// 處理所有移動相關的計算和物理應用
// ============================================================================

using System;
using Nobeta.EUG.FSM.Interfaces;
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

        // 構造函數注入
        [Inject]
        public MovementService(
            CharacterController controller,
            Transform transform,
            ICameraService cameraService,
            ITimeService timeService)
        {
            this.controller = controller;
            this.transform = transform;

            this.timeService = timeService;
            _cameraService = cameraService;
            velocity = Vector3.zero;
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

        public void RotateTowards(Vector3 direction, float rotationSpeed)
        {
            //GetHorizonOffset
            Debug.Log($"{DateTime.Now}direction: {direction}, rotationSpeed: {rotationSpeed},GetHorizonOffset:{_cameraService.GetHorizonOffset}");
            if (direction.magnitude < 0.1f) return;

            // 計算目標旋轉
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 平滑旋轉
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                timeService.DeltaTime * (rotationSpeed+ _cameraService.GetHorizonOffset*50)
            );
            //transform.rotation = targetRotation;
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