using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Pathfinding
{
    public class SquareGrid : IGrid
    {
        public float Width { get => _width; }
        public float Length { get => _length; }
        public float Size { get => _size; }

        public Vector3 Origin { get => _origin; }
        public List<Vector3> WorldPos { get => _worldPos; }

        private float _width;
        private float _length;
        private float _size;
        private Vector3 _origin;
        private List<Vector3> _worldPos = new List<Vector3>();
        private List<Vector3> oriPoint = new List<Vector3>();

        [Inject]
        public void Construct (GridConfig config)
        {
            oriPoint = config.oriPoints;
            _width = config.width;
            _length = config.length;
            _size = config.size;
            _origin = config.origin;
            HashSet<Vector3> pointSet = new HashSet<Vector3>();

            for (int i = 0; i < oriPoint.Count; i++) //
            {
                foreach (var point in GenerateGrid(oriPoint[i])) //
                {
                    pointSet.Add(point);
                }
            }

            // Convert back to List if other systems (like NodeData) require it
            _worldPos = new List<Vector3>(pointSet);
        }

        public List<Vector3> GenerateGrid(Vector3 startPos)
        {
            List<Vector3> grid = new List<Vector3>();

            for (int x = -(int)((_width/_size)/2); x <= (int)((_width/_size)/2); x++)
            {
                for (int z = -(int)((_length / _size) / 2); z <= ((int)(_length / _size) / 2) ; z++)
                {
                    Vector3 point = new Vector3(
                        startPos.x + x * _size,
                        startPos.y,
                        startPos.z + z * _size
                    );
                    grid.Add(point);
                }
            }
            return grid;
        }
    }
}