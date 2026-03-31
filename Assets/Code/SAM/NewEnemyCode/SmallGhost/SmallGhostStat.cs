using UnityEngine;
using Nobeta.Sam.Enemy;

/// <summary>
/// SmallGhost-specific stat asset.
/// Extends EnemyStat with SG-exclusive movement tuning.
/// Registered in SGLifetimeScope as both SmallGhostStat and EnemyStat.
/// </summary>
[CreateAssetMenu(fileName = "SmallGhostStat", menuName = "Scriptable Objects/SmallGhostStat")]
public class SmallGhostStat : EnemyStat
{
    [Header("SmallGhost Movement")]
   
    public float ChaseWeight = 0.7f; // 0 = pure noise drift, 1 = beeline toward player
}
