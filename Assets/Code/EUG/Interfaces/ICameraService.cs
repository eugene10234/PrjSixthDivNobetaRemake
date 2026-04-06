// ========================================================================
// Camera Service - 攝影機服務接口
// ========================================================================
using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Nobeta.EUG.Interfaces
{
    /// <summary>
    /// 攝影機服務接口 - 提供攝影機相關信息
    /// </summary>
    public interface ICameraService
    {
        /// <summary>
        /// 獲取攝影機相對方向
        /// </summary>
        Vector3 GetCameraRelativeDirection(Vector2 input);

        /// <summary>
        /// 獲取攝影機前方向（用於瞄準）
        /// </summary>
        Vector3 GetCameraForward();

        /// <summary>
        /// 獲取攝影機右方向
        /// </summary>
        Vector3 GetCameraRight();
        void Initial(CinemachineThirdPersonAim cinemachineThirdPersonAim);
        void Initial(CinemachineCamera cinemachineCamera);
        void LateUpdate();
        bool TryGetCameraSphereHit(float v1, float v2, out RaycastHit hitInfo, LayerMask mask = default);
        (Vector3, Vector3) GetAimPosition();
        float GetHorizonOffset { get; }
    }
}


