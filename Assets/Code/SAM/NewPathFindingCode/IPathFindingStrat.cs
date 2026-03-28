using System.Collections.Generic;
using UnityEngine;

namespace Nobeta.Sam.Pathfinding
{
    public interface IPathFindingStrat
    {
        public List<Vector3> CalculatePath(Vector3 start, Vector3 end);
    }
}
