using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
using UnityEngine;
using VContainer;
// ========================================================================
// BulletFactory (工廠類 - Pure C#)
// ========================================================================
namespace Nobeta.EUG.BulletSystem.OOP
{
    /// <summary>
    /// 子彈工廠
    /// 職責：創建和配置子彈
    /// </summary>
    public class BulletFactory
    {
        private readonly IObjectResolver container;
        private int nextId = 0;

        public BulletFactory(IObjectResolver container)
        {
            this.container = container;
        }

        /// <summary>
        /// 創建子彈（從 VContainer 解析）
        /// </summary>
        public Bullet Create()
        {
            return container.Resolve<Bullet>();
        }

        /// <summary>
        /// 創建並初始化子彈
        /// </summary>
        public Bullet CreateAndInitialize(
            BulletConfig config,
            Vector3 position,
            Vector3 direction)
        {
            var bullet = Create();

            config.Id = nextId++;
            bullet.Initialize(config, position, direction);

            return bullet;
        }
    }

}