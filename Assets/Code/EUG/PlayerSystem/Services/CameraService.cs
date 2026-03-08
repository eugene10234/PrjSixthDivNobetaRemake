using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Interfaces;
using System;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;
// ========================================================================
// CameraService - 攝影機服務
// ========================================================================

namespace Nobeta.EUG.FSM.Services
{
    public class CameraService : ICameraService, IDisposable
    {
        private float m_Pitch = 0f; // 上下旋轉
        private float m_Yaw = 0f;   // 左右旋轉
        private Transform m_followPoint;
        private readonly Camera mainCamera;
        private float mHorizontalSensetivity = 1.5f;
        private float mVerticalSensetivity = 1f;
        public float GetFloat=> m_Yaw;

        [Inject]
        public CameraService()
        {
            mainCamera = Camera.main;
            AssignInputEvents();
        }

        public Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            if (mainCamera == null) return new Vector3(input.x, 0, input.y);

            Vector3 forward = mainCamera.transform.forward;
            Vector3 right = mainCamera.transform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return forward * input.y + right * input.x;
        }

        public Vector3 GetCameraForward()
        {
            if (mainCamera == null) return Vector3.forward;

            Vector3 forward = mainCamera.transform.forward;
            forward.y = 0f;
            return forward.normalized;
        }

        public Vector3 GetCameraRight()
        {
            if (mainCamera == null) return Vector3.right;

            Vector3 right = mainCamera.transform.right;
            right.y = 0f;
            return right.normalized;
        }
        public void Initial(CinemachineCamera CinemachineCamera)
        {
            m_followPoint = CinemachineCamera.Target.TrackingTarget;
            // 讓初始角度等於當前物體的角度，避免瞬移
            Vector3 currentEuler = m_followPoint.eulerAngles;
            m_Yaw = currentEuler.y;
            m_Pitch = currentEuler.x;
            // 注意：Euler x 有時會超過 180 度，若有問題需做 Mathf.DeltaAngle 轉換

            //player = GetComponent<mbPlayer>();

        }
        public void ExecuteCameraRotation()
        {
            // 1. 取得並累加輸入值
            m_Yaw += Input.GetAxis("Mouse X") * mHorizontalSensetivity;
            m_Pitch -= Input.GetAxis("Mouse Y") * mVerticalSensetivity; // 通常 Y 軸輸入需要反轉

            // 2. 限制垂直角度（防止翻轉）
            // 假設 0 是水平，90 是看腳下，-90 是看天空
            m_Pitch = Mathf.Clamp(m_Pitch, -89f, 89f);

            // 3. 直接計算新的四元數旋轉
            // 使用 Euler 組合出新的旋轉，這不會受上一幀的旋轉偏移影響
            Quaternion targetRotation = Quaternion.Euler(m_Pitch, m_Yaw, 0f);

            // 4. 更新 _desireForward (只取水平方向，用於角色轉身邏輯)
            // 這裡我們只使用 m_Yaw，忽略 m_Pitch
            var _desireForward = Quaternion.Euler(0f, m_Yaw, 0f) * Vector3.forward;

            // 5. 套用旋轉
            m_followPoint.rotation = targetRotation;
        }

        public void Dispose()
        {
            UnAssignInputEvents();
        }

        private void AssignInputEvents()
        {
            //throw new NotImplementedException();
        }

        private void UnAssignInputEvents()
        {
            //throw new NotImplementedException();
        }

        public void Initial(CinemachineThirdPersonAim cinemachineThirdPersonAim)
        {
            throw new NotImplementedException();
        }
    }
}
