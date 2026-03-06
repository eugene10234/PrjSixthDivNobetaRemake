using System;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace Nobeta.Scripts.PlayerSpace
{
    public class PlayerAim : MonoBehaviour
    {
        //private IPlayer player;


        [Header("Aim Viusal - Laser")]
        [SerializeField] private LineRenderer aimLaser; // this component is on the waepon holder(child of a player)

        [Header("Aim control")]
        [SerializeField] private Transform aim;

        [SerializeField] private bool isAimingPrecisly;
        [SerializeField] private bool isLockingToTarget;

        [Header("Camera control")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private Transform mFollowPoint;

        private Vector3 _desireForward;
        private Vector3 _desireAngleVector = new Vector3(0,0,1f);
        [SerializeField] private float mHorizontalSensetivity = 5f;
        [SerializeField] private float mVerticalSensetivity = 2f;
        [Range(.5f, 1)]
        [SerializeField] private float minCameraDistance = 1.5f;
        [Range(1, 3f)]
        [SerializeField] private float maxCameraDistance = 4;
        [Range(3f, 5f)]
        [SerializeField] private float cameraSensetivity = 5f;

        [Space]

        [SerializeField] private LayerMask aimLayerMask;

        private Vector2 mouseInput;
        private RaycastHit lastKnownMouseHit;

        private void Start()
        {
            // 讓初始角度等於當前物體的角度，避免瞬移
            Vector3 currentEuler = mFollowPoint.eulerAngles;
            m_Yaw = currentEuler.y;
            m_Pitch = currentEuler.x;
            // 注意：Euler x 有時會超過 180 度，若有問題需做 Mathf.DeltaAngle 轉換

            //player = GetComponent<mbPlayer>();
            AssignInputEvents();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
                isAimingPrecisly = !isAimingPrecisly;

            if (Input.GetKeyDown(KeyCode.L))
                isLockingToTarget = !isLockingToTarget;
            //Debug.Log("s"+DateTime.Now+GetMouseHitInfo().point);
            //UpdateAimVisuals();
            //UpdateAimPosition();
            //UpdateCameraPosition();
            //UpdateCameraRotation();
        }
        private void LateUpdate()
        {
            UpdateCameraRotation();
        }
        // 在類別頂部定義私有變數，用來記錄當前角度
        private float m_Pitch = 0f; // 上下旋轉
        private float m_Yaw = 0f;   // 左右旋轉
        private void UpdateCameraRotation()
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
            _desireForward = Quaternion.Euler(0f, m_Yaw, 0f) * Vector3.forward;

            // 5. 套用旋轉
            mFollowPoint.rotation = targetRotation;
        }

        //private void UpdateAimVisuals()
        //{
        //    Transform gunPoint = player.weapon.GunPoint();
        //    Vector3 laserDirection = player.weapon.BulletDirection();

        //    float laserTipLenght = .5f;
        //    float gunDistance = 4f;

        //    Vector3 endPoint = gunPoint.position + laserDirection * gunDistance;

        //    if (Physics.Raycast(gunPoint.position, laserDirection, out RaycastHit hit, gunDistance))
        //    {
        //        endPoint = hit.point;
        //        laserTipLenght = 0;
        //    }

        //    aimLaser.SetPosition(0, gunPoint.position);
        //    aimLaser.SetPosition(1, endPoint);
        //    aimLaser.SetPosition(2, endPoint + laserDirection * laserTipLenght);
        //}
        //private void UpdateAimPosition()
        //{
        //    Transform target = Target();

        //    if (target != null && isLockingToTarget)
        //    {
        //        aim.position = target.position;
        //        return;
        //    }

        //    aim.position = GetMouseHitInfo().point;

        //    if (!isAimingPrecisly)
        //        aim.position = new Vector3(aim.position.x, transform.position.y + 1, aim.position.z);
        //}




        //public Transform Target()
        //{
        //    Transform target = null;

        //    if (GetMouseHitInfo().transform.GetComponent<Target>() != null)
        //    {
        //        target = GetMouseHitInfo().transform;
        //    }

        //    return target;
        //}
        public Transform Aim() => aim;
        public bool CanAimPrecisly() => isAimingPrecisly;
        public Vector2 GetMouseInfo()
        {
            return mouseInput;
        }
        public Vector3 GetDesireForward()
        {
            Vector3 flattenedForward = Vector3.ProjectOnPlane(mFollowPoint.forward, Vector3.up).normalized;
            return _desireForward;
        }
        public Vector3 GetDesireAngleVector()
        {
            return _desireAngleVector;
        }
        public RaycastHit GetMouseHitInfo()
        {
            Ray ray = Camera.main.ScreenPointToRay(mouseInput);
           
            if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, aimLayerMask))
            {
                //Debug.Log("Hit: " + hitInfo.transform.name + hitInfo.transform.position +DateTime.Now);
           
                lastKnownMouseHit = hitInfo;
                return hitInfo;
            }

            return lastKnownMouseHit;
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lastKnownMouseHit.point, .2f);
        }
        #region Camera Region

        //private void UpdateCameraPosition()
        //{
        //    cameraTarget.position =
        //                Vector3.Lerp(cameraTarget.position, DesieredCameraPosition(), cameraSensetivity * Time.deltaTime);
        //}

        //private Vector3 DesieredCameraPosition()
        //{
        //    float actualMaxCameraDistance = player.movement.moveInput.y < -.5f ? minCameraDistance : maxCameraDistance;

        //    Vector3 desiredCameraPosition = GetMouseHitInfo().point;
        //    Vector3 aimDirection = (desiredCameraPosition - transform.position).normalized;

        //    float distanceToDesierdPosition = Vector3.Distance(transform.position, desiredCameraPosition);
        //    float clampedDistance = Mathf.Clamp(distanceToDesierdPosition, minCameraDistance, actualMaxCameraDistance);

        //    desiredCameraPosition = transform.position + aimDirection * clampedDistance;
        //    desiredCameraPosition.y = transform.position.y + 1;

        //    return desiredCameraPosition;
        //}

        #endregion

        private void AssignInputEvents()
        {
            //controls = new PlayerControls();

            //controls.Character.Aim.performed += context => mouseInput = context.ReadValue<Vector2>();
            //controls.Character.Aim.canceled += context => mouseInput = Vector2.zero;
        }

    }
}