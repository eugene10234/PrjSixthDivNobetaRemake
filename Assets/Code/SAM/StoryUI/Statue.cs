using Nobeta.EUG.Interfaces;
using Nobeta.Sam.Events;
using UnityEngine;

public class Statue : MonoBehaviour
{
    public int statueID;
    public Transform spawnPoint;
    private Vector3 spawnLoc;
    private IEventBus _eventBus;
    private void Awake()
    {
        SetGround();
        spawnLoc = spawnPoint.position;
        _eventBus = EventBusProvider.GetEventBus();
    }

    public void SetGround()
    {
        // 1. 取得 Layer 的索引並轉換為 Bitmask
        // 這裡會同時偵測 "Ground" 或 "Obstacle" 層
        int layerMask = LayerMask.GetMask("Ground", "Obstacle");

        // 2. 設定射線參數
        float rayOffset = 0.5f; // 從中心往上提一點，避免穿模
        float maxDistance = 20f; // 往下偵測的距離
        Vector3 rayStart = transform.position + Vector3.up * rayOffset;

        // 3. 執行 Raycast
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, maxDistance, layerMask))
        {
            spawnPoint.position = hit.point;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            _eventBus.Publish(new DialogEvent { ID = statueID });
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            _eventBus.Publish(new DialogDisableEvent());
        }
    }
}
