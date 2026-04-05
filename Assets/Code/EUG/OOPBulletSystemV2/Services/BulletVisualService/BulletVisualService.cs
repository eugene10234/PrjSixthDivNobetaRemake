using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.STP;

namespace Nobeta.EUG.BulletSystemV2.OOP.UnityBridge
{
    public class BulletVisualService
    {
        // ── 每種 BulletType 的 VFX 配置 ──────────────────────────────────
        private readonly Dictionary<BulletType, BulletVFXConfig> _vfxMap = new();

        // ── 子彈視覺物件池：BulletState.Id → GameObject ───────────────────
        //private readonly Dictionary<int, GameObject> _activeVisuals = new();

        // ── 回收池（依 BulletType 分組）──────────────────────────────────
        //private readonly Dictionary<BulletType, Stack<GameObject>> _visualPool = new();
        private readonly BulletVFXConfig[] vfxConfigs;
        private readonly Transform visualContainerRoot;


        private readonly Dictionary<BulletType, Stack<GameObject>> bulletVisualPool = new();
        private readonly Dictionary<BulletVFXConfig, BulletVisualPoolData> VisualSystem = new();
        private readonly Dictionary<Bullet, BulletVFXInstance> activeBulletVFX = new();
        // ✨ UniTask：取消令牌
        private readonly CancellationTokenSource disposeCts = new CancellationTokenSource();

        public BulletVisualService(BulletVFXConfig[] configs, Transform visualRoot)
        {
            vfxConfigs = configs;
            visualContainerRoot = visualRoot;

            //activeBulletVFX = new Dictionary<Bullet, BulletVFXInstance>();

            int typeCount = configs.Length;

            // 初始化視覺池
            //bulletVisualPools = new Stack<GameObject>[typeCount];

            //// 初始化特效池
            //muzzleFlashPools = new Stack<GameObject>[typeCount];
            //trailPools = new Stack<GameObject>[typeCount];
            //hitEffectPools = new Stack<GameObject>[typeCount];
            foreach (var cfg in configs)
            {
                _vfxMap[cfg.Type] = cfg;
                VisualSystem[cfg] = new BulletVisualPoolData(cfg, visualContainerRoot);
                VisualSystem[cfg].PrewarmPools(30);
                //_visualPool[cfg.Type] = new Stack<GameObject>();
            }
        }
        // ── 每幀同步視覺位置（由 Bridge.Update 呼叫）──────────────────────

        public void SyncVisualsToLogic(IReadOnlyList<Bullet> activeBullets)
        {
            foreach (var bullet in activeBullets)
            {
                // 為新子彈分配視覺
                if (!activeBulletVFX.ContainsKey(bullet))
                    AssignVisual(bullet);
                // 同步位置 / 旋轉
                if (activeBulletVFX.TryGetValue(bullet, out var go) && go != null)
                {
                    go.bulletVisual.transform.position = bullet.Position;
                    go.bulletVisual.transform.rotation = bullet.Rotation;
                }
            }
        }

        // ── 分配視覺 ──────────────────────────────────────────────────────

        private void AssignVisual(Bullet bullet)
        {
            var type = bullet.State.Type;
            if (!_vfxMap.TryGetValue(type, out var cfg)) return;
            if (!VisualSystem.TryGetValue(cfg, out var cfgPoolRoot)) return;
            // 從池取或新建
            var vfxInstance = new BulletVFXInstance();

            // 1. 分配子彈視覺
            vfxInstance.bulletVisual = GetBulletVisual(cfg);
            if (vfxInstance.bulletVisual != null)
            {
                SetupBulletVisual(vfxInstance.bulletVisual, bullet);
            }
            //// 2. 播放槍口閃光
            PlayMuzzleFlashAsync(cfg, bullet.Position, bullet.Rotation).Forget();  // ✨ UniTask

            // 3. 分配拖尾效果
            if (cfg.TrailRendererPrefab != null)
            {
                vfxInstance.trail = GetTrail(cfg);
                if (vfxInstance.trail != null)
                {
                    SetupTrail(vfxInstance.trail, vfxInstance.bulletVisual, bullet);
                }
            }
            activeBulletVFX[bullet] = vfxInstance;
            //_activeVisuals[bullet.Id] = go;

            // 播槍口閃光（fire-and-forget）
            //PlayMuzzleFlashAsync(cfg, bullet.Position, bullet.Rotation).Forget();
        }
        #region Trail（拖尾）

        private GameObject GetTrail(BulletVFXConfig cfg)
        {
            var cfgPoolRoot = VisualSystem[cfg];
            GameObject trail;
            if (cfgPoolRoot.TrailRender.Count > 0)
            {
                trail = cfgPoolRoot.TrailRender.Pop();
            }
            else
            {
                trail = Object.Instantiate(cfg.TrailRendererPrefab, cfgPoolRoot.rootTransform);
            }
            return trail;
        }

        private void SetupTrail(GameObject trail, GameObject bulletVisual, Bullet bullet)
        {
            trail.transform.position = bullet.Position;
            trail.transform.rotation = bullet.Rotation;
            trail.transform.SetParent(bulletVisual.transform);
            trail.SetActive(true);

            foreach (var ps in trail.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Clear();
                ps.Play();
            }

            foreach (var tr in trail.GetComponentsInChildren<TrailRenderer>())
            {
                tr.Clear();
            }
        }

        #endregion

        private GameObject GetBulletVisual(BulletVFXConfig cfg)
        {
            // 嘗試從字典中取得對應類型的 Pool 資料
            if (VisualSystem.TryGetValue(cfg, out var cfgPoolRoot))
            {
                if (cfgPoolRoot.BulletVisual.Count > 0)
                {
                    GameObject bulletVisual = cfgPoolRoot.BulletVisual.Pop();
                    return bulletVisual;
                }
            }
            if (cfg.BulletVisualPrefab != null)
            {
                return Object.Instantiate(cfg.BulletVisualPrefab, cfgPoolRoot.rootTransform);
            }
            return new GameObject($"Bullet_{cfg.Type}");
        }
        private void SetupBulletVisual(GameObject visual, Bullet bullet)
        {
            visual.transform.position = bullet.Position;
            visual.transform.rotation = bullet.Rotation;
            visual.SetActive(true);

            foreach (var ps in visual.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Clear();
                ps.Play();
            }

            foreach (var tr in visual.GetComponentsInChildren<TrailRenderer>())
            {
                tr.Clear();
            }
        }
        // ── 回收視覺（含 Trail 延遲）──────────────────────────────────────

        public void RecycleVisual(Bullet bullet)
        {
            if (!activeBulletVFX.TryGetValue(bullet, out var vfxInstance)) return;
            if (!_vfxMap.TryGetValue(bullet.State.Type, out var cfg)) return;
            if (!VisualSystem.TryGetValue(cfg, out var cfgPoolRoot)) return;

            // 1. 回收子彈視覺
            if (vfxInstance.bulletVisual != null)
            {
                RecycleBulletVisual(vfxInstance.bulletVisual, cfg);
            }

            // 2. 處理拖尾（延遲回收）- UniTask 版本 ✨
            if (vfxInstance.trail != null)
            {
                RecycleTrailAsync(vfxInstance.trail, cfg).Forget();
            }
            activeBulletVFX.Remove(bullet);
        }
        private void RecycleBulletVisual(GameObject visual, BulletVFXConfig cfg)
        {
            var cfgPoolRoot = VisualSystem[cfg];
            foreach (var ps in visual.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            foreach (var tr in visual.GetComponentsInChildren<TrailRenderer>())
            {
                tr.Clear();   
            }
            visual.SetActive(false);
            cfgPoolRoot.BulletVisual.Push(visual);
        }
        /// <summary>
        /// 延遲回收拖尾（UniTask 異步）✨
        /// </summary>
        private async UniTaskVoid RecycleTrailAsync(GameObject trail, BulletVFXConfig cfg)
        {
            if (trail == null) return;
            var cfgPoolRoot = VisualSystem[cfg];
            // 解除父子關係
            trail.transform.SetParent(cfgPoolRoot.rootTransform);

            //trail.SetActive(true);
            ////停止 TrailRenderer 繼續發射新點
            //foreach (var tr in trail.GetComponentsInChildren<TrailRenderer>())
            //{
            //    tr.emitting = false;
            //}
            // 停止發射新粒子
            foreach (var ps in trail.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }

            // ✨ UniTask：異步延遲
            try
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(cfg.trailLifetime),
                    cancellationToken: disposeCts.Token
                );

                // 回收
                if (trail != null)
                {
                    //現在 trail 是 active，能正確找到 TrailRenderer
                    foreach (var tr in trail.GetComponentsInChildren<TrailRenderer>())
                    {
                        tr.Clear();
                        //tr.emitting = true;  // 為下次使用重置
                    }
                    foreach (var ps in trail.GetComponentsInChildren<ParticleSystem>())
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }

                    trail.SetActive(false);
                    cfgPoolRoot.TrailRender.Push(trail);
                }
            }
            catch (System.OperationCanceledException)
            {
                // 取消時清理
                if (trail != null)
                {
                    Object.Destroy(trail);
                }
            }
        }

        public async UniTaskVoid PlayHitEffectAsync(BulletHitEvent evt)
        {
            if (!_vfxMap.TryGetValue(evt.Type, out var cfg)) return;
            if (cfg.HitEffectPrefab == null) return;

            GameObject hitEffect;
            if (!VisualSystem.TryGetValue(cfg, out var cfgPoolRoot)) return;
            if (cfgPoolRoot.HitEffect.Count > 0)
            {
                hitEffect = cfgPoolRoot.HitEffect.Pop();
            }
            else
            {
                hitEffect = Object.Instantiate(cfg.HitEffectPrefab, cfgPoolRoot.rootTransform);
            }

            hitEffect.transform.position = evt.HitPosition;
            hitEffect.transform.rotation = Quaternion.LookRotation(evt.HitNormal);
            hitEffect.SetActive(true);

            foreach (var ps in hitEffect.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Clear();
                ps.Play();
            }

            // ✨ UniTask：異步延遲
            try
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(cfg.hitEffectLifetime),
                    cancellationToken: disposeCts.Token
                );

                // 回收
                if (hitEffect != null)
                {
                    hitEffect.SetActive(false);
                    cfgPoolRoot.HitEffect.Push(hitEffect);
                }
            }
            catch (System.OperationCanceledException)
            {
                // 取消時清理
                if (hitEffect != null)
                {
                    Object.Destroy(hitEffect);
                }
            }
        }

        // ── Private helpers ───────────────────────────────────────────────

        private async UniTaskVoid PlayMuzzleFlashAsync(BulletVFXConfig cfg, Vector3 pos, Quaternion rot)
        {
            if (cfg.MuzzleFlashPrefab == null) return;
            var cfgPoolRoot = VisualSystem[cfg];
            GameObject muzzle;
            if (cfgPoolRoot.MuzzleEffect.Count > 0)
            {
                muzzle = cfgPoolRoot.MuzzleEffect.Pop();
            }
            else
            {
                muzzle = Object.Instantiate(cfg.MuzzleFlashPrefab, cfgPoolRoot.rootTransform);
            }
            muzzle.transform.position = pos;
            muzzle.transform.rotation = rot;
            muzzle.SetActive(true);

            foreach (var ps in muzzle.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Clear();
                ps.Play();
            }

            // ✨ UniTask：異步延遲
            try
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(cfg.muzzleFlashLifetime),
                    cancellationToken: disposeCts.Token
                );

                // 回收
                if (muzzle != null)
                {
                    muzzle.SetActive(false);
                    cfgPoolRoot.MuzzleEffect.Push(muzzle);
                }
            }
            catch (System.OperationCanceledException)
            {
                // 取消時清理
                if (muzzle != null)
                {
                    Object.Destroy(muzzle);
                }
            }

        }

        #region Dispose（清理）✨ UniTask

        /// <summary>
        /// 清理資源（取消所有異步操作）
        /// </summary>
        public void Dispose()
        {
            // 取消所有進行中的異步操作
            disposeCts?.Cancel();
            disposeCts?.Dispose();
        }

        #endregion

        // ── 命中特效（依 HitType 播放不同特效，這是解答你問題的核心）──────
        /// <summary>
        /// 播放擊中特效（UniTask 異步）
        /// </summary>

        public async UniTaskVoid PlayHitEffectStategyBadAsyncExample(BulletHitEvent evt)
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
    }
}