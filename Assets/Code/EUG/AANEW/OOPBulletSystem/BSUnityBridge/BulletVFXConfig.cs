using System.Collections.Generic;
using Nobeta.EUG.BulletSystem.OOP;
//using Nobeta.EUG.Core.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;
// ========================================================================
// BulletVFXConfig - VFX 配置
// ========================================================================
namespace Nobeta.EUG.BulletSystem.OOP.UnityBridge
{
    /// <summary>
    /// 子彈 VFX 配置
    /// </summary>
    [System.Serializable]
    public class BulletVFXConfig
    {
        [Header("基礎視覺")]
        public GameObject bulletVisualPrefab;  // 子彈本體

        [Header("槍口特效 Muzzle")]
        public GameObject muzzleFlashPrefab;   // 槍口閃光
        public float muzzleFlashLifetime = 1f;

        [Header("拖尾特效 Trail")]
        public GameObject trailPrefab;         // 拖尾粒子
        public float trailLifetime = 2f;       // 子彈銷毀後拖尾停留時間

        [Header("擊中特效 BulletHit")]
        public GameObject hitEffectPrefab;     // 擊中特效
        public float hitEffectLifetime = 1f;
    }
}


