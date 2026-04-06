using VContainer;
using Nobeta.Sam.Enemy;
using System.Collections.Generic;
using UnityEngine;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// MageGhost runtime state.
    /// Extends BaseEnemyData — add MG-exclusive runtime fields here if needed.
    ///
    /// Registered in MGLifetimeScope as both MageGhostData and BaseEnemyData so:
    ///   - Shared states inject BaseEnemyData       (read HP, Target, AvoidanceDirection)
    ///   - MG-specific code injects MageGhostData (access any MG-exclusive fields)
    /// </summary>
    public class MageGhostData : BaseEnemyData
    {
        public string Name = "憎恨的黑夜生魂";
        public string Reward = "雷擊魔法";

        [Inject]
        public MageGhostData(MageGhostStat stat) : base(stat) { }

        public bool hasSummoned { get; set; }

        public List<GameObject> minionSpawnLoc = new List<GameObject>();

    }
}
