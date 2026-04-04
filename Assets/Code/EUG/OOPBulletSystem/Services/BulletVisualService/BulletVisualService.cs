using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Nobeta.EUG.BulletSystemV2.OOP.UnityBridge
{
    public class BulletVisualService
    {
        // ── 每種 BulletType 的 VFX 配置 ──────────────────────────────────
        private readonly Dictionary<BulletType, BulletVFXConfig> _vfxMap = new();

        // ── 子彈視覺物件池：BulletState.Id → GameObject ───────────────────
        private readonly Dictionary<int, GameObject> _activeVisuals = new();

        // ── 回收池（依 BulletType 分組）──────────────────────────────────
        private readonly Dictionary<BulletType, Stack<GameObject>> _visualPool = new();

        private readonly Transform _visualRoot;

        public BulletVisualService(BulletVFXConfig[] configs, Transform visualRoot)
        {
            _visualRoot = visualRoot;

            foreach (var cfg in configs)
            {
                _vfxMap[cfg.Type] = cfg;
                _visualPool[cfg.Type] = new Stack<GameObject>();
            }
        }

        // ── 每幀同步視覺位置（由 Bridge.Update 呼叫）──────────────────────

        public void SyncVisualsToLogic(IReadOnlyList<Bullet> activeBullets)
        {
            // 為新子彈分配視覺
            foreach (var bullet in activeBullets)
            {
                if (!_activeVisuals.ContainsKey(bullet.Id))
                    AssignVisual(bullet);
            }

            // 同步位置 / 旋轉
            foreach (var bullet in activeBullets)
            {
                if (_activeVisuals.TryGetValue(bullet.Id, out var go) && go != null)
                {
                    go.transform.position = bullet.Position;
                    go.transform.rotation = bullet.Rotation;
                }
            }
        }

        // ── 分配視覺 ──────────────────────────────────────────────────────

        private void AssignVisual(Bullet bullet)
        {
            var type = bullet.State.Type;
            if (!_vfxMap.TryGetValue(type, out var cfg)) return;

            // 從池取或新建
            GameObject go = GetOrCreateVisual(type, cfg);
            go.SetActive(true);
            go.transform.position = bullet.Position;
            go.transform.rotation = bullet.Rotation;

            _activeVisuals[bullet.Id] = go;

            // 播槍口閃光（fire-and-forget）
            PlayMuzzleFlashAsync(cfg, bullet.Position, bullet.Rotation).Forget();
        }

        // ── 回收視覺（含 Trail 延遲）──────────────────────────────────────

        public async UniTaskVoid RecycleVisual(Bullet bullet)
        {
            if (!_activeVisuals.TryGetValue(bullet.Id, out var go))
                return;

            _activeVisuals.Remove(bullet.Id);

            if (go == null) return;

            // 停止移動，等待拖尾消散
            var trail = go.GetComponentInChildren<TrailRenderer>();
            if (trail != null)
            {
                trail.emitting = false;
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(trail.time),
                    cancellationToken: go.GetCancellationTokenOnDestroy()
                );
            }

            ReturnVisualToPool(bullet.State.Type, go);
        }

        // ── 命中特效（依 HitType 播放不同特效，這是解答你問題的核心）──────

        public async UniTaskVoid PlayHitEffectAsync(BulletHitEvent evt)
        {
            if (!_vfxMap.TryGetValue(evt.Type, out var cfg)) return;

            GameObject prefab = evt.HitType switch
            {
                BulletHitType.Single => cfg.HitEffectSingle,
                BulletHitType.Aoe => cfg.HitEffectAoe,
                BulletHitType.Dot => cfg.HitEffectDot,
                _ => cfg.HitEffectSingle
            };

            if (prefab == null) return;

            var hitGo = Object.Instantiate(prefab, evt.HitPosition, Quaternion.LookRotation(evt.HitNormal));

            // AoE：把特效 Scale 成對應爆炸半徑
            if (evt.HitType == BulletHitType.Aoe && evt.AoeRadius > 0f)
            {
                float scale = evt.AoeRadius * 2f;
                hitGo.transform.localScale = Vector3.one * scale;
            }

            // 等 ParticleSystem 播完自動銷毀
            var ps = hitGo.GetComponentInChildren<ParticleSystem>();
            float duration = ps != null
                ? ps.main.duration + ps.main.startLifetime.constantMax
                : 2f;

            await UniTask.Delay(
                System.TimeSpan.FromSeconds(duration),
                cancellationToken: hitGo.GetCancellationTokenOnDestroy()
            );

            if (hitGo != null)
                Object.Destroy(hitGo);
        }

        // ── Private helpers ───────────────────────────────────────────────

        private async UniTaskVoid PlayMuzzleFlashAsync(BulletVFXConfig cfg, Vector3 pos, Quaternion rot)
        {
            if (cfg.MuzzleFlashPrefab == null) return;

            var flash = Object.Instantiate(cfg.MuzzleFlashPrefab, pos, rot);
            var ps = flash.GetComponent<ParticleSystem>();
            float dur = ps != null ? ps.main.duration : 0.2f;

            await UniTask.Delay(System.TimeSpan.FromSeconds(dur));
            if (flash != null) Object.Destroy(flash);
        }

        private GameObject GetOrCreateVisual(BulletType type, BulletVFXConfig cfg)
        {
            if (_visualPool.TryGetValue(type, out var pool) && pool.Count > 0)
                return pool.Pop();

            return cfg.BulletVisualPrefab != null
                ? Object.Instantiate(cfg.BulletVisualPrefab, _visualRoot)
                : new GameObject($"Bullet_{type}");
        }

        private void ReturnVisualToPool(BulletType type, GameObject go)
        {
            var trail = go.GetComponentInChildren<TrailRenderer>();
            if (trail != null)
            {
                trail.Clear();
                trail.emitting = true;
            }

            go.SetActive(false);

            if (!_visualPool.ContainsKey(type))
                _visualPool[type] = new Stack<GameObject>();

            _visualPool[type].Push(go);
        }
    }
}