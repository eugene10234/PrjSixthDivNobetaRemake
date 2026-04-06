using DG.Tweening;
using Nobeta.EUG.BulletSystemV1.OOP.UnityBridge;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using VContainer;


public class MageAim : MonoBehaviour
{
    //[Header("目標設定")]
    //public Transform target; // 想要看的目標

    [SerializeField] GameObject GameObjectActive;
    private Animator _playerAnimator;
    private readonly float HandChant_Duration = 0.5f;
    private bool isPlayerLive = true;
    private bool isChanting = false;
    private IEventBus _eventBus;

    private void Awake()
    {
        _eventBus = EventBusProvider.GetEventBus();
        GameObjectActive.SetActive(false);
    }
    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }
    public Vector3 targetPosition;
    public float speed = 5f;
    private Tween chaseTween;

    // 開始追逐
    public void ChaseStart(Vector3 TargetPosition, Vector3 TargetNormal)
    {
        targetPosition = TargetPosition;
        //SetGround(targetPosition);

        // 如果原本就有在追逐，先殺掉舊的
        ChaseEnd();
        if (targetPosition == null) return;
        // 使用 DOMove 追逐目標位置
        // 每 0.1 秒更新一次路徑，並保持循環
        chaseTween = transform.DOMove(targetPosition, 0.1f)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                // 1. 生命週期檢查
                if (!isPlayerLive)
                {
                    // 注意：在 Tween 執行中直接 OnDestroy() 可能會噴錯
                    // 建議先 Kill 掉 Tween
                    chaseTween.Kill();
                    Destroy(gameObject);
                    return;
                }

                //// 2. 處理旋轉 (假設 TargetNormal 是動態變化的)
                //// 這裡讓物件的「上方」對齊 TargetNormal，這是最常見的貼地/法線對齊做法
                //if (TargetNormal != Vector3.zero)
                //{
                //    // 使用平滑插值，讓旋轉看起來不那麼生硬
                //    Quaternion targetRot = Quaternion.FromToRotation(Vector3.up, TargetNormal);
                //    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
                //}

                // 3. 如果有目標，動態更新終點
                //if (target != null)
                //{
                //    chaseTween.ChangeEndValue(target.position, true);
                //}
            });
            //.SetLoops(-1); // 無限循環
    }

    // 結束並殺死 Tween
    public void ChaseEnd()
    {
        if (chaseTween != null && chaseTween.IsActive())
        {
            chaseTween.Kill();
            chaseTween = null;
        }
    }
    private void OnMageAimUpdateStart(MageAimUpdateEvent evt)
    {
        GameObjectActive.SetActive(true);
        ChaseStart(evt.TargetPosition,evt.TargetNormal);
    }
    private void OnChaseEnd(ChaseAimEndEvent evt)
    {
        GameObjectActive.SetActive(false);
    }
    private void OnDestroy()
    {
        ChaseEnd(); // 養成好習慣，銷毀時清理 Tween
    }
    private void OnPlayerLiveStateChange(PlayerLiveStateEvent evt)
    {
        isPlayerLive = evt.IsLive;
    }
    private void SubscribeEvents()
    {
        _eventBus.Subscribe<PlayerLiveStateEvent>(OnPlayerLiveStateChange);
        _eventBus.Subscribe<MageAimUpdateEvent>(OnMageAimUpdateStart);
        _eventBus.Subscribe<ChaseAimEndEvent>(OnChaseEnd);
    }
    private void UnsubscribeEvents()
    {
        _eventBus.Unsubscribe<PlayerLiveStateEvent>(OnPlayerLiveStateChange);
        _eventBus.Unsubscribe<MageAimUpdateEvent>(OnMageAimUpdateStart);
        _eventBus.Unsubscribe<ChaseAimEndEvent>(OnChaseEnd);
    }
    //public void SetGround(Vector3 targetPosition)
    //{
    //    // 1. 取得 Layer 的索引並轉換為 Bitmask
    //    // 這裡會同時偵測 "Ground" 或 "Obstacle" 層
    //    int layerMask = LayerMask.GetMask("Ground", "Obstacle");

    //    // 2. 設定射線參數
    //    float rayOffset = 0.5f; // 從中心往上提一點，避免穿模
    //    float maxDistance = 20f; // 往下偵測的距離
    //    Vector3 rayStart = targetPosition + Vector3.up * rayOffset;

    //    // 3. 執行 Raycast
    //    if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, maxDistance, layerMask))
    //    {
    //        // 移動到碰撞點
    //        targetPosition = hit.point;

    //        // 偵錯用：在 Scene 視窗畫出一條紅線
    //        Debug.DrawLine(rayStart, hit.point, Color.green, 2.0f);
    //    }
    //    else
    //    {
    //        // 偵錯用：如果沒打到，畫一條紅線表示範圍
    //        Debug.DrawRay(rayStart, Vector3.down * maxDistance, Color.red, 2.0f);
    //        Debug.LogWarning($"{transform.name} 下方沒有找到 Ground 或 Obstacle 層的物體。");
    //    }
    //}

}
