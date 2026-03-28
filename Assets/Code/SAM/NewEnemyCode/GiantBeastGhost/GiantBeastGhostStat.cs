using UnityEngine;
using Nobeta.Sam.Enemy;

/// <summary>
/// GiantBeastGhost-specific stat asset.
/// Extends EnemyStat with fields that only GiantBeastGhost needs.
/// Registered in GBGLifetimeScope as both GiantBeastGhostStat and EnemyStat so shared states
/// can inject EnemyStat while GBG-specific code can inject GiantBeastGhostStat.
/// </summary>
[CreateAssetMenu(fileName = "GiantBeastGhostStat", menuName = "Scriptable Objects/GiantBeastGhostStat")]
public class GiantBeastGhostStat : EnemyStat
{
    // No extra fields currently — GiantBeastGhost uses only the base stats.
    // Add GBG-exclusive fields here as the design evolves, e.g.:
    // public float ShieldDuration;
    // public float RoarRadius;
    public float dodgeSpeed;
}
