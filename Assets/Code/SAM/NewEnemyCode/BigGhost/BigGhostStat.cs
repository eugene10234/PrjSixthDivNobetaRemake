using UnityEngine;
using Nobeta.Sam.Enemy;

/// <summary>
/// BigGhost-specific stat asset.
/// Extends EnemyStat with fields that only BigGhost needs.
/// Registered in BGLifetimeScope as both BigGhostStat and EnemyStat so shared states
/// can inject EnemyStat while BG-specific code can inject BigGhostStat.
/// </summary>
[CreateAssetMenu(fileName = "BigGhostStat", menuName = "Scriptable Objects/BigGhostStat")]
public class BigGhostStat : EnemyStat
{
    // No extra fields currently — BigGhost uses only the base stats.
    // Add BG-exclusive fields here as the design evolves, e.g.:
    // public float ShieldDuration;
    // public float RoarRadius;
}
