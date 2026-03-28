// ========================================================================
// Movement Service - 移動服務接口
// ========================================================================
using System;
using UnityEngine;

namespace Nobeta.EUG.FSM.Interfaces
{
    /// <summary>
    /// 移動服務接口 - 處理所有與移動相關的計算和應用
    /// 職責：計算速度、應用物理、處理碰撞
    /// </summary>
    public interface IMovementService
    {
        /// <summary>
        /// 當前速度
        /// </summary>
        Vector3 CurrentVelocity { get; }

        /// <summary>
        /// 當前水平速度
        /// </summary>
        float CurrentHorizontalSpeed { get; }

        /// <summary>
        /// 應用地面移動
        /// </summary>
        /// <param name="direction">移動方向（歸一化）</param>
        /// <param name="targetSpeed">目標速度</param>
        /// <param name="acceleration">加速度</param>
        void ApplyGroundMovement(Vector3 direction, float targetSpeed, float acceleration);

        /// <summary>
        /// 應用空中移動（有限控制）
        /// </summary>
        /// <param name="direction">移動方向</param>
        /// <param name="targetSpeed">目標速度</param>
        /// <param name="airControlFactor">空中控制係數 (0-1)</param>
        void ApplyAirMovement(float previousSpeed, Vector3 previousdirection, Vector3 direction, float targetSpeed, float airControlFactor);

        /// <summary>
        /// 應用重力
        /// </summary>
        /// <param name="gravity">重力加速度</param>
        void ApplyGravity(float gravity);
        void CancelGravity();
        void ReduceGravity(float reduceAmount);
        /// <summary>
        /// 應用跳躍
        /// </summary>
        /// <param name="jumpHeight">跳躍高度</param>
        /// <param name="gravity">重力</param>
        void ApplyJump(float jumpHeight, float gravity);

        /// <summary>
        /// 應用閃避移動
        /// </summary>
        /// <param name="direction">閃避方向</param>
        /// <param name="distance">閃避距離</param>
        /// <param name="duration">閃避時長</param>
        void ApplyDodge(Vector3 direction, float distance, float duration);

        /// <summary>
        /// 旋轉角色朝向指定方向
        /// </summary>
        /// <param name="direction">目標方向</param>
        /// <param name="rotationSpeed">旋轉速度</param>
        void RotateTowards(Vector3 direction, float rotationSpeed);
        public void RotateTowardsImmediate(Vector3 direction);


        /// <summary>
        /// 固定速度平滑旋轉(不受相機影響)
        /// </summary>
        public void RotateTowardsSteady(Vector3 direction, float rotationSpeed);
        /// <summary>
        /// 停止移動（平滑減速）
        /// </summary>
        /// <param name="deceleration">減速度</param>
        void StopMovement(float deceleration);

        /// <summary>
        /// 重置速度
        /// </summary>
        void ResetVelocity();
        void remainVelocity(float offset);
        /// <summary>
        /// 設置垂直速度（用於著地等情況）
        /// </summary>
        void SetVerticalVelocity(float verticalVelocity);
        
        /// <summary>
        /// 執行移動（應用到 CharacterController）
        /// </summary>
        void ExecuteMovement();
        void SetGround();
        void RootMotionStart();
        void RootMotionEnd();
        void RootMotionClear();
        
    }

    
}


