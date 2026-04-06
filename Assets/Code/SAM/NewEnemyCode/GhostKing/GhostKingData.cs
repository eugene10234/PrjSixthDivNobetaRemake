using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Runtime shared data bag for the Ghost King — mirrors MageGhostData.
    /// Holds target reference and avoidance vector consumed by states.
    /// </summary>
    public class GhostKingData : BaseEnemyData
    {
        // BaseEnemyData already carries Target and AvoidanceDirection.
        // Add any GK-specific runtime fields here as the boss design grows.
        public string Name = "被遺忘的生魂之主";
        public string Reward = "珍貴的記憶";

        [Inject]
        public GhostKingData(NewGKStat stat) : base(stat) 
        {
            Debug.Log("GhostKing Data Ready");
        }

        public bool hasSummoned { get; set; }

        public List<GameObject> minionSpawnLoc = new List<GameObject>();

        public float stageOneHPThreshold = 0.75f;
        public float stageTwoHPThreshold = 0.25f;
    }
}
