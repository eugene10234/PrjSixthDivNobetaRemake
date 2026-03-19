using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.Interfaces;
using System;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;
// ========================================================================
// CameraService - 攝影機服務
// ========================================================================

namespace Nobeta.EUG.Services
{
    public class CameraService : ICameraService, IDisposable, IUpdatable, ILateUpdatable
    {
        private float m_Pitch = 0f; // 上下旋轉
        private float m_Yaw = 0f;   // 左右旋轉
        private float horizonOffset = 0f;
        private CinemachineCamera _cinemachineCamera;
        private CinemachineThirdPersonFollow _cinemachineThirdPersonFollow;
        private Transform m_followPoint;
        private readonly IPlayerInputService _playerInputService;
        private readonly Camera mainCamera;
        private float mHorizontalSensetivity = 1.5f;
        private float mVerticalSensetivity = 1f;
        private readonly static float DefaultFOV = 50f;
        private readonly static float DefaultCamDistance = 2f;
        private readonly static float DefaultShoulderHeight = -0.11f;
        private float _targetFOV = DefaultFOV;        
        private float _targetDistance = DefaultCamDistance;
        private float _targetShoulderOffset = DefaultShoulderHeight;
        private Vector3 _targetShoulder = new Vector3(0.5f, DefaultShoulderHeight, 0);
        private float _smoothFOVSpeed = 0.2f;


        public float GetHorizonOffset => horizonOffset;
        public float FieldOfView { get => _cinemachineCamera.Lens.FieldOfView; set => _cinemachineCamera.Lens.FieldOfView = value; }
        public float CinCameraDistance { get => _cinemachineThirdPersonFollow.CameraDistance; set => _cinemachineThirdPersonFollow.CameraDistance= value; }
        [Inject]
        public CameraService(IPlayerInputService playerInputService)
        {
            _playerInputService = playerInputService;
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
            _cinemachineCamera = CinemachineCamera;
            _cinemachineThirdPersonFollow = _cinemachineCamera.GetComponent<CinemachineThirdPersonFollow>();
            m_followPoint = CinemachineCamera.Target.TrackingTarget;
            _targetShoulder = new Vector3(0.5f, DefaultShoulderHeight, 0);
            // 讓初始角度等於當前物體的角度，避免瞬移
            Vector3 currentEuler = m_followPoint.eulerAngles;
            m_Yaw = currentEuler.y;
            m_Pitch = currentEuler.x;
            // 注意：Euler x 有時會超過 180 度，若有問題需做 Mathf.DeltaAngle 轉換

            //player = GetComponent<mbPlayer>();

        }
        private void ExecuteCameraRotation()
        {
            var mouseMoveOffset = _playerInputService.MousePosOffset;
            //Debug.Log($"Mouse Move Offset11: {Input.GetAxis("Mouse X")}:{Input.GetAxis("Mouse Y")}");
            float x = mouseMoveOffset.x / 20f;//Input.GetAxis("Mouse X")
            float y = mouseMoveOffset.y / 20f;//Input.GetAxis("Mouse Y")
            //Debug.Log($"Mouse Move Offset22: {mouseMoveOffset.x}:{mouseMoveOffset.y}");
            // 1. 取得並累加輸入值
            horizonOffset = x * mHorizontalSensetivity;
            m_Yaw += horizonOffset;
            m_Pitch -= y * mVerticalSensetivity; // 通常 Y 軸輸入需要反轉

            // 2. 限制垂直角度（防止翻轉）
            // 假設 0 是水平，90 是看腳下，-90 是看天空
            //Debug.Log($"Raw Pitch: {m_Pitch}");
            m_Pitch = Mathf.Clamp(m_Pitch, -55f, 55f);

            // 3. 直接計算新的四元數旋轉
            // 使用 Euler 組合出新的旋轉，這不會受上一幀的旋轉偏移影響
            Quaternion targetRotation = Quaternion.Euler(m_Pitch, m_Yaw, 0f);

            // 4. 更新 _desireForward (只取水平方向，用於角色轉身邏輯)
            // 這裡我們只使用 m_Yaw，忽略 m_Pitch
            var _desireForward = Quaternion.Euler(0f, m_Yaw, 0f) * Vector3.forward;

            // 5. 套用旋轉
            m_followPoint.rotation = targetRotation;
        }
        private void ExecuteCameraZoom()
        {
            // 這裡的 smoothSpeed 決定了相機跟隨目標 FOV 的速度
            FieldOfView = Mathf.Lerp(FieldOfView, _targetFOV, _smoothFOVSpeed);
            CinCameraDistance = Mathf.Lerp(CinCameraDistance, _targetDistance, _smoothFOVSpeed*0.6f);
            _cinemachineThirdPersonFollow.ShoulderOffset = Vector3.Lerp(_cinemachineThirdPersonFollow.ShoulderOffset, SetTargerShoulderValue(_targetShoulder), _smoothFOVSpeed);

        }
        public Vector3 SetTargerShoulderValue(Vector3 targetShoulder)
        {
            targetShoulder.y = Mathf.Lerp(targetShoulder.y, _targetShoulderOffset, _targetFOV / DefaultFOV);

            return targetShoulder;

        }
        public void Dispose()
        {
            UnAssignInputEvents();
        }

        private void AssignInputEvents()
        {
            _playerInputService.OnAimStart += OnCameraAimStart;
            _playerInputService.OnAimEnd += OnCameraAimEnd;
        }

        private void UnAssignInputEvents()
        {
            _playerInputService.OnAimStart -= OnCameraAimStart;
            _playerInputService.OnAimEnd -= OnCameraAimEnd;
        }

        public void Initial(CinemachineThirdPersonAim cinemachineThirdPersonAim)
        {
            throw new NotImplementedException();
        }
        private void OnCameraAimStart()
        {
            //throw new NotImplementedException();
            _targetFOV = 57.5f; // 縮小視野，模擬瞄準效果
            _targetDistance = 0.9f; // 拉近相機距離，模擬瞄準效果
            _targetShoulderOffset = -0.38f;
        }
        private void OnCameraAimEnd()
        {
            //throw new NotImplementedException();
            _targetFOV = DefaultFOV; // 縮小視野，模擬瞄準效果
            _targetDistance = DefaultCamDistance; // 拉近相機距離，模擬瞄準效果
            _targetShoulderOffset = DefaultShoulderHeight;
        }

        public void LateUpdate()
        {
            ExecuteCameraZoom();
            ExecuteCameraRotation();
        }
        public bool TryGetCameraSphereHit(float radius, float maxDistance, out RaycastHit hit, LayerMask mask = default)
        {
            // 取得相機中心射線
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (mask == default) mask = ~0;

            // 執行 SphereCast
            return Physics.SphereCast(ray, radius, out hit, maxDistance, mask);
        }
        public void Update()
        {
            throw new NotImplementedException();
        }

    }
}
