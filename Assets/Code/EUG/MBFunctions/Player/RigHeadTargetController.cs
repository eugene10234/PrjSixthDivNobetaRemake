using System;
using Nobeta.EUG.BulletSystem.OOP.UnityBridge;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using VContainer;


public class RigHeadTargetController : MonoBehaviour
{
    //[Header("目標設定")]
    //public Transform target; // 想要看的目標

    [Header("Rig 設定")]
    public Rig rig;                   // 你的 Rig 元件
    public Transform rigTarget;      // 你的 Animation Rigging Target 物件
    public Transform headBone;     // 頭部骨骼（用來當作發射基準點）
    public Transform target2222; // 想要看的目標
    public float targetDistance = 3f; // Target 離頭部的距離（建議 2~5 之間）

    [Header("角度限制")]
    public float minYaw = -175f;
    public float maxYaw = 175f;
    public float minPitch = -60f;
    public float maxPitch = 60f;

    [Header("平滑設定")]
    public float smoothTime = 0.4f;
    public float returnSmoothTime = 0.8f;
    public float weightSpeed = 20f;    // 權重增減的速度

    private float currentYaw;
    private float currentPitch;
    private float yawVelocity;
    private float pitchVelocity;
    [Inject] private readonly IPlayerInputService _playerInputService;
    [Inject] private readonly ITimeService _timeService;
    [Inject] private ICameraService _cameraService;
    [Inject] private Animator _animator;
    [Inject] private readonly IEventBus _eventBus;
    private float LookingTimer = 0f;
    private const float LOOKING_DURATION = 0.25f;
    private float targetWeight = 0f;
    private float targetweightSpeed = 0f;
    private bool isPlayerLive = true;
    private void OnEnable()
    {
        _eventBus.Subscribe<PlayerLiveStateEvent>(OnPlayerLiveStateChange);
    }
    private void OnDisable()
    {
        _eventBus.Unsubscribe<PlayerLiveStateEvent>(OnPlayerLiveStateChange);
    }
    private void OnPlayerLiveStateChange(PlayerLiveStateEvent evt)
    {
        isPlayerLive = evt.IsLive;
    }
    void Update()
    {
        if (!isPlayerLive) return;
        bool isLooking = _playerInputService.IsFirePressed || _playerInputService.IsAimPressed;
        if (isLooking) LookingTimer = LOOKING_DURATION;
        LookingTimer -= Time.deltaTime;
        //bool hasTarget = target != null;
        if (LookingTimer > 0.01f)
        {
            targetWeight = 1f;
            targetweightSpeed = weightSpeed * Time.fixedDeltaTime;
        }
        else
        {
            targetWeight = 0f;
            targetweightSpeed = 2f * weightSpeed * Time.deltaTime;
        }

        // 1. 處理 Rig 權重的平滑切換 (Fade In/Out)
        if (rig != null)
        {
            rig.weight = Mathf.MoveTowards(rig.weight, targetWeight, targetweightSpeed);
            _animator.SetLayerWeight(2, rig.weight * 0.7f);
        }

        if (rigTarget == null || headBone == null) return;

        float targetYaw = 0f;
        float targetPitch = 0f;
        float currentSmooth = returnSmoothTime;
        Vector3 targetPoint = transform.position + Camera.main.transform.forward * 50f;
        if (_cameraService.TryGetCameraSphereHit(0.2f, 50f, out RaycastHit hitInfo))
        {
            targetPoint = hitInfo.point;

        }
        // 1. 計算目標角度
        if (true) // 假設你有一個外部傳入的 target Transform//target != null
        {
            // 注意：這裡使用 headBone 的父物件作為參考坐標系
            Vector3 localTarget = headBone.parent.InverseTransformPoint(targetPoint);

            targetYaw = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
            float horizontalDistance = new Vector2(localTarget.x, localTarget.z).magnitude;
            targetPitch = Mathf.Atan2(localTarget.y, horizontalDistance) * Mathf.Rad2Deg;
            //======================


            targetYaw = Mathf.Clamp(targetYaw, minYaw, maxYaw);
            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
            target2222.localPosition = GetCustomValue(targetYaw);
            targetYaw = targetYaw * 1.28f;
            currentSmooth = smoothTime;
        }
        // 3. 【關鍵修改】使用一般的 SmoothDamp 而非 Angle 版本
        // Mathf.SmoothDamp 不知道角度可以環繞，所以它會乖乖地從目前的數值「線性」滑向目標值
        // 假設目前在 90，目標在 -90，它一定會經過 80, 70... 0 ... -70, -80, -90
        currentYaw = Mathf.SmoothDamp(currentYaw, targetYaw, ref yawVelocity, smoothTime);
        currentPitch = Mathf.SmoothDamp(currentPitch, targetPitch, ref pitchVelocity, smoothTime);

        // 4. 套用至 RigTarget
        Quaternion lookRotation = Quaternion.Euler(-currentPitch, currentYaw, 0f);
        Vector3 targetLocalPos = lookRotation * Vector3.forward * targetDistance;
        rigTarget.position = headBone.parent.TransformPoint(targetLocalPos);
        //// 2. 平滑計算當前角度
        //currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, currentSmooth);
        //currentPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref pitchVelocity, currentSmooth);

        //// 3. 將角度轉換為「空間中的位移點」
        //// 我們建立一個以 headBone 為中心的方向向量
        //Quaternion lookRotation = Quaternion.Euler(-currentPitch, currentYaw, 0f);

        //// 核心邏輯：在 headBone 的前方 Distance 處計算出一個世界座標
        //// 我們先計算在 headBone 父物件空間下的方向，再轉回世界座標
        //Vector3 direction = lookRotation * Vector3.forward;
        //Vector3 targetLocalPos = direction * targetDistance;

        //// 將該點設定給 RigTarget
        //rigTarget.position = headBone.parent.TransformPoint(targetLocalPos);
    }

    // 為了方便你在編輯器看 Target 是否正確，加個 Gizmos
    //private void OnDrawGizmos()
    //{
    //    if (rigTarget != null)
    //    {
    //        Gizmos.color = Color.yellow;
    //        Gizmos.DrawWireSphere(rigTarget.position, 0.1f);
    //        Gizmos.DrawLine(headBone.position, rigTarget.position);
    //    }
    //}
    /// <summary>
    /// 根據 Yaw 角度計算對應的平滑數值
    /// </summary>
    /// <param name="yaw">輸入的角度 (-180 到 180)</param>
    /// <returns>輸出的數值 (-0.36 到 0.36)</returns>
    public Vector3 GetCustomValue(float yaw)
    {
        //Debug.Log($"GetCustomValue called with yaw: {yaw}");
        // 確保輸入在 -180 到 180 之間
        float normalizedYaw = Mathf.Clamp(yaw, -180f, 180f);

        if (normalizedYaw <= 0)
        {
            // 0 ~ -90 度：恆定為 0.36
            // 0 ~ 90 度：從 0.36 平滑過渡到 -0.36
            // 使用 t = normalizedYaw / -180f，當 yaw 為 0 時 t=0，當 yaw 為 90 時 t=1
            float t = normalizedYaw / -90f;
            //var easeout = 1 - Mathf.Pow(1f - t, 2);
            //Mathf.Lerp(0.33f, -0.33f, t);
            //0.069f
            //Mathf.Lerp(0.40f, 0.65f, t);
            return new Vector3(Mathf.Lerp(0f, -0.33f, t), 0.069f, Mathf.Lerp(0.50f, 0.243f, t));
            //return 0.42f;
        }
        else
        {
            // 0 ~ 90 度：從 0.36 平滑過渡到 -0.36
            // 使用 t = normalizedYaw / -180f，當 yaw 為 0 時 t=0，當 yaw 為 90 時 t=1
            float t = normalizedYaw / 90f;
            //var easeout = 1 - Mathf.Pow(1f - t, 2);
            //Mathf.Lerp(0.33f, 0.33f, t);
            //0.069f
            //Mathf.Lerp(0.40f, -0.36f, t);
            return new Vector3(Mathf.Lerp(0f, 0.43f, t), 0.069f, Mathf.Lerp(0.50f, -0.46f, t));
        }
    }
}