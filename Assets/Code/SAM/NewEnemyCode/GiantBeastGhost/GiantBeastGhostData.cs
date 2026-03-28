using VContainer;
using Nobeta.Sam.Enemy;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// GiantBeastGhost runtime state.
    /// Extends BaseEnemyData — add GBG-exclusive runtime fields here if needed.
    ///
    /// Registered in GBGLifetimeScope as both GiantBeastGhostData and BaseEnemyData so:
    ///   - Shared states inject BaseEnemyData       (read HP, Target, AvoidanceDirection)
    ///   - GBG-specific code injects GiantBeastGhostData (access any GBG-exclusive fields)
    /// </summary>
    public class GiantBeastGhostData : BaseEnemyData
    {
        [Inject]
        public GiantBeastGhostData(GiantBeastGhostStat stat) : base(stat) { }
    }
}
