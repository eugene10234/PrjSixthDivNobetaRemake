using System.Collections.Generic;
using UnityEngine;

namespace Nobeta.Sam.Pathfinding
{
    public interface IGrid
    {
        public float Width { get; }
        public float Length { get; }
        public float Size { get; }

        public Vector3 Origin { get; }
        public List<Vector3> WorldPos { get;}
        public List<Vector3> GenerateGrid(Vector3 startPos);
    }
}