using VContainer;
using Nobeta.Sam.Enemy;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// BigGhost runtime state.
    /// Extends BaseEnemyData — add BG-exclusive runtime fields here if needed.
    ///
    /// Registered in BGLifetimeScope as both BigGhostData and BaseEnemyData so:
    ///   - Shared states inject BaseEnemyData  (read HP, Target, AvoidanceDirection)
    ///   - BG-specific code injects BigGhostData (access any BG-exclusive fields)
    /// </summary>
    public class BigGhostData : BaseEnemyData
    {
        [Inject]
        public BigGhostData(BigGhostStat stat) : base(stat) { }
    }
}
