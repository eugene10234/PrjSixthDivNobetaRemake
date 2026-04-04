using Cysharp.Threading.Tasks;
using Nobeta.EUG.BulletSystemV1.OOP;
//using Nobeta.EUG.Core.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;
using VContainer.Unity;
// ========================================================================
// BulletVisualService - UniTask 版本
// ========================================================================
namespace Nobeta.EUG.BulletSystemV1.OOP.UnityBridge
{
    /// <summary>
        /// 子彈視覺服務（UniTask 版本）
        /// 
        /// 優勢：
        /// - ✅ 不需要 MonoBehaviour（CoroutineRunner）
        /// - ✅ 更好的性能（UniTask 零分配）
        /// - ✅ 可取消（CancellationToken）
        /// - ✅ 更簡潔的異步代碼
        /// - ✅ 支持 async/await
        /// </summary>
    public class BulletVisualService : System.IDisposable
        {
            #region Private Fields

            // 配置
            private readonly BulletVFXConfig[] vfxConfigs;

            // 視覺池
            private readonly Stack<GameObject>[] bulletVisualPools;
            private readonly Transform visualContainer;

            // 特效池
            private readonly Stack<GameObject>[] muzzleFlashPools;
            private readonly Stack<GameObject>[] trailPools;
            private readonly Stack<GameObject>[] hitEffectPools;

            // 活動映射
            private readonly Dictionary<Bullet, BulletVFXInstance> activeBulletVFX;

            // ✨ UniTask：取消令牌
            private readonly CancellationTokenSource disposeCts = new CancellationTokenSource();

            #endregion

            #region Constructor

            public BulletVisualService(BulletVFXConfig[] configs, Transform container)
            {
                vfxConfigs = configs;
                visualContainer = container;

                activeBulletVFX = new Dictionary<Bullet, BulletVFXInstance>();

                int typeCount = configs.Length;

                // 初始化視覺池
                bulletVisualPools = new Stack<GameObject>[typeCount];

                // 初始化特效池
                muzzleFlashPools = new Stack<GameObject>[typeCount];
                trailPools = new Stack<GameObject>[typeCount];
                hitEffectPools = new Stack<GameObject>[typeCount];

                // 預熱所有池
                for (int i = 0; i < typeCount; i++)
                {
                    bulletVisualPools[i] = new Stack<GameObject>();
                    muzzleFlashPools[i] = new Stack<GameObject>();
                    trailPools[i] = new Stack<GameObject>();
                    hitEffectPools[i] = new Stack<GameObject>();

                    PrewarmPools(i, 20);
                }
            }

            #endregion

            #region Prewarm

            private void PrewarmPools(int type, int count)
            {
                var config = vfxConfigs[type];

                // 預熱子彈視覺
                if (config.bulletVisualPrefab != null)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var visual = Object.Instantiate(config.bulletVisualPrefab, visualContainer);
                        visual.SetActive(false);
                        bulletVisualPools[type].Push(visual);
                    }
                }

                // 預熱槍口閃光
                if (config.muzzleFlashPrefab != null)
                {
                    for (int i = 0; i < count / 2; i++)
                    {
                        var muzzle = Object.Instantiate(config.muzzleFlashPrefab, visualContainer);
                        muzzle.SetActive(false);
                        muzzleFlashPools[type].Push(muzzle);
                    }
                }

                // 預熱拖尾
                if (config.trailPrefab != null)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var trail = Object.Instantiate(config.trailPrefab, visualContainer);
                        trail.SetActive(false);
                        trailPools[type].Push(trail);
                    }
                }

                // 預熱擊中特效
                if (config.hitEffectPrefab != null)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var hit = Object.Instantiate(config.hitEffectPrefab, visualContainer);
                        hit.SetActive(false);
                        hitEffectPools[type].Push(hit);
                    }
                }
            }

            #endregion

            #region Assign Visual（分配視覺）

            public void AssignVisual(Bullet bullet)
            {
                int type = (int)bullet.Type;
                var config = vfxConfigs[type];

                var vfxInstance = new BulletVFXInstance();

                // 1. 分配子彈視覺
                vfxInstance.bulletVisual = GetBulletVisual(type);
                if (vfxInstance.bulletVisual != null)
                {
                    SetupBulletVisual(vfxInstance.bulletVisual, bullet);
                }

                // 2. 播放槍口閃光
                PlayMuzzleFlashAsync(type, bullet.Position, bullet.Rotation).Forget();  // ✨ UniTask

                // 3. 分配拖尾效果
                if (config.trailPrefab != null)
                {
                    vfxInstance.trail = GetTrail(type);
                    if (vfxInstance.trail != null)
                    {
                        SetupTrail(vfxInstance.trail, vfxInstance.bulletVisual, bullet);
                    }
                }

                activeBulletVFX[bullet] = vfxInstance;
            }

            private GameObject GetBulletVisual(int type)
            {
                GameObject visual;

                if (bulletVisualPools[type].Count > 0)
                {
                    visual = bulletVisualPools[type].Pop();
                }
                else
                {
                    visual = Object.Instantiate(vfxConfigs[type].bulletVisualPrefab, visualContainer);
                }

                return visual;
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

            #endregion

            #region Muzzle Flash（槍口閃光）- UniTask 版本 ✨

            /// <summary>
            /// 播放槍口閃光效果（UniTask 異步）
            /// </summary>
            public async UniTaskVoid PlayMuzzleFlashAsync(int type, Vector3 position, Quaternion rotation)
            {
                var config = vfxConfigs[type];
                if (config.muzzleFlashPrefab == null)
                    return;

                GameObject muzzle;

                if (muzzleFlashPools[type].Count > 0)
                {
                    muzzle = muzzleFlashPools[type].Pop();
                }
                else
                {
                    muzzle = Object.Instantiate(config.muzzleFlashPrefab, visualContainer);
                }

                muzzle.transform.position = position;
                muzzle.transform.rotation = rotation;
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
                        System.TimeSpan.FromSeconds(config.muzzleFlashLifetime),
                        cancellationToken: disposeCts.Token
                    );

                    // 回收
                    if (muzzle != null)
                    {
                        muzzle.SetActive(false);
                        muzzleFlashPools[type].Push(muzzle);
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

            #endregion

            #region Trail（拖尾）

            private GameObject GetTrail(int type)
            {
                GameObject trail;

                if (trailPools[type].Count > 0)
                {
                    trail = trailPools[type].Pop();
                }
                else
                {
                    trail = Object.Instantiate(vfxConfigs[type].trailPrefab, visualContainer);
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

            #region Recycle Visual（回收視覺）

            public void RecycleVisual(Bullet bullet)
            {
                if (!activeBulletVFX.TryGetValue(bullet, out var vfxInstance))
                    return;

                int type = (int)bullet.Type;
                var config = vfxConfigs[type];

                // 1. 回收子彈視覺
                if (vfxInstance.bulletVisual != null)
                {
                    RecycleBulletVisual(vfxInstance.bulletVisual, type);
                }

                // 2. 處理拖尾（延遲回收）- UniTask 版本 ✨
                if (vfxInstance.trail != null)
                {
                    RecycleTrailAsync(vfxInstance.trail, type, config).Forget();
                }

                activeBulletVFX.Remove(bullet);
            }

            private void RecycleBulletVisual(GameObject visual, int type)
            {
                foreach (var ps in visual.GetComponentsInChildren<ParticleSystem>())
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                foreach (var tr in visual.GetComponentsInChildren<TrailRenderer>())
                {
                    tr.Clear();
                }

                visual.SetActive(false);
                bulletVisualPools[type].Push(visual);
            }

            /// <summary>
            /// 延遲回收拖尾（UniTask 異步）✨
            /// </summary>
            private async UniTaskVoid RecycleTrailAsync(GameObject trail, int type, BulletVFXConfig config)
            {
                if (trail == null)
                    return;

                // 解除父子關係
                trail.transform.SetParent(visualContainer);

                // 停止發射新粒子
                foreach (var ps in trail.GetComponentsInChildren<ParticleSystem>())
                {
                    ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                }

                // ✨ UniTask：異步延遲
                try
                {
                    await UniTask.Delay(
                        System.TimeSpan.FromSeconds(config.trailLifetime),
                        cancellationToken: disposeCts.Token
                    );

                    // 回收
                    if (trail != null)
                    {
                        foreach (var ps in trail.GetComponentsInChildren<ParticleSystem>())
                        {
                            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        }

                        foreach (var tr in trail.GetComponentsInChildren<TrailRenderer>())
                        {
                            tr.Clear();
                        }

                        trail.SetActive(false);
                        trailPools[type].Push(trail);
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

            #endregion

            #region Hit Effect（擊中特效）- UniTask 版本 ✨

            /// <summary>
            /// 播放擊中特效（UniTask 異步）
            /// </summary>
            public async UniTaskVoid PlayHitEffectAsync(int type, Vector3 position, Vector3 normal)
            {
                var config = vfxConfigs[type];
                if (config.hitEffectPrefab == null)
                    return;

                GameObject hitEffect;

                if (hitEffectPools[type].Count > 0)
                {
                    hitEffect = hitEffectPools[type].Pop();
                }
                else
                {
                    hitEffect = Object.Instantiate(config.hitEffectPrefab, visualContainer);
                }

                hitEffect.transform.position = position;
                hitEffect.transform.rotation = Quaternion.LookRotation(normal);
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
                        System.TimeSpan.FromSeconds(config.hitEffectLifetime),
                        cancellationToken: disposeCts.Token
                    );

                    // 回收
                    if (hitEffect != null)
                    {
                        hitEffect.SetActive(false);
                        hitEffectPools[type].Push(hitEffect);
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

            #endregion

            #region Sync（同步）

            public void SyncVisualsToLogic(IReadOnlyList<Bullet> bullets)
            {
                foreach (var bullet in bullets)
                {
                    if (!activeBulletVFX.TryGetValue(bullet, out var vfxInstance))
                    {
                        AssignVisual(bullet);
                        continue;
                    }

                    if (vfxInstance.bulletVisual != null)
                    {
                        vfxInstance.bulletVisual.transform.position = bullet.Position;
                        vfxInstance.bulletVisual.transform.rotation = bullet.Rotation;
                    }
                }
            }

            #endregion

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
        }
    }


