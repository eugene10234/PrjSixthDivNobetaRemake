using VContainer;
using Nobeta.Sam.Enemy;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// SmallGhost runtime state.
    /// Extends BaseEnemyData — add SG-exclusive runtime fields here if needed.
    /// </summary>
    public class SmallGhostData : BaseEnemyData
    {
        [Inject]
        public SmallGhostData(SmallGhostStat stat) : base(stat) { }
    }
}
