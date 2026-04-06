using DG.Tweening;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public class BulletConfigRegistry
    {
        private readonly Dictionary<WeaponConfigType, BulletConfig> _configs;

        public BulletConfigRegistry(BulletConfigFactory factory)
        {
            _configs = new Dictionary<WeaponConfigType, BulletConfig>
            {
                [WeaponConfigType.Normal] = factory.Normal(),
                [WeaponConfigType.IceBall] = factory.IceBall(),
                [WeaponConfigType.FireBall] = factory.FireBall(),
                [WeaponConfigType.LightningStrike] = factory.LightningStrike(),
                [WeaponConfigType.BurnBomb] = factory.BurnBomb(),
                [WeaponConfigType.SpecialNormal] = factory.SpecialNormal(),
            };
        }

        public BulletConfig Get(WeaponConfigType type) => _configs[type];
    }
}