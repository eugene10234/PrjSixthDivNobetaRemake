using System.Collections.Generic;
using Nobeta.EUG.BulletSystem.OOP;
//using Nobeta.EUG.Core.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;
// ========================================================================
// BulletVFXInstance - VFX 實例
// ========================================================================
namespace Nobeta.EUG.BulletSystem.OOP.UnityBridge
{
    /// <summary>
    /// 子彈 VFX 實例（包含所有特效引用）
    /// </summary>
    internal class BulletVFXInstance
    {
        public GameObject bulletVisual;  // 子彈本體
        public GameObject trail;         // 拖尾粒子
    }
}


