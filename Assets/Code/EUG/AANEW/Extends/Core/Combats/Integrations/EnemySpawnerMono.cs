using UnityEngine;
using VContainer;
using GDCO.Combat.Entities;

namespace GDCO.Combat.Integration
{
    /// <summary>
    /// 掛載在 Unity 場景中的 Enemy 生成控制器
    /// </summary>
    public class EnemySpawnerMono : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private float defaultMaxHealth = 1000f;

        // 透過 VContainer 注入 Pure C# 的 Spawner 服務
        private IEnemySpawner _enemySpawner;

        /// <summary>
        /// VContainer 的 MonoBehaviour 注入點
        /// </summary>
        [Inject]
        public void Construct(IEnemySpawner enemySpawner)
        {
            _enemySpawner = enemySpawner;
        }

        /// <summary>
        /// 提供給 Unity 事件（如 Button、Trigger）或內部邏輯呼叫的生成方法
        /// </summary>
        public IEnemy SpawnAtCurrentPosition()
        {
            if (_enemySpawner == null)
            {
                Debug.LogError($"[{name}] IEnemySpawner 未成功注入！請檢查 VContainer LifetimeScope 設置。");
                return null;
            }

            if (enemyPrefab == null)
            {
                Debug.LogError($"[{name}] Enemy Prefab 未指派！");
                return null;
            }

            // 呼叫你的 Pure C# Spawner 進行組裝
            return _enemySpawner.SpawnEnemy(enemyPrefab, transform.position, defaultMaxHealth);
        }

        /// <summary>
        /// 測試用的快捷鍵生成（可自由刪除）
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                SpawnAtCurrentPosition();
            }
        }
    }
}