using UnityEngine;
using Nobeta.Sam.Enemy;

/// <summary>
/// MageGhost-specific stat asset.
/// Extends EnemyStat with fields that only MageGhost needs.
/// Registered in MGLifetimeScope as both MageGhostStat and EnemyStat so shared states
/// can inject EnemyStat while MG-specific code can inject MageGhostStat.
/// </summary>
[CreateAssetMenu(fileName = "MageGhostStat", menuName = "Scriptable Objects/MageGhostStat")]
public class MageGhostStat : EnemyStat
{
    // No extra fields currently — MageGhost uses only the base stats.
    // Add MG-exclusive fields here as the design evolves, e.g.:
    // public float ShieldDuration;
    // public float RoarRadius;
}
