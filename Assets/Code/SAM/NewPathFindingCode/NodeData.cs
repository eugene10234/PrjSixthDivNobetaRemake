using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.Sam.Pathfinding
{
    public class NodeData: IStartable
    {
        public Dictionary<Vector3, Node> AllNodes { get; private set; }
        public Dictionary<(int, int), Node> Coordinates { get; private set; }

        public Dictionary<Vector3, Node> DynamicList = new Dictionary<Vector3, Node>();

        public const float enemyPenalty = 1;

        private IGrid _Grid;

        [Inject]
        public void Construct(IGrid currentGrid)
        {
            _Grid = currentGrid;
            AllNodes = new Dictionary<Vector3, Node>();
        }
        public void Start()
        {
            InitializeNodes();
        }
       
        public void InitializeNodes()
        {
            //add all nodes into a collection
            foreach (var point in _Grid.WorldPos)
            {
                AllNodes.Add(point, new Node(point, enemyPenalty));
            }
            //check validity
            foreach (var node in AllNodes.Values)
            {
                node.isValid = true;

                if (Physics.CheckSphere(node.position, _Grid.Size/1.2f, 1 << LayerMask.NameToLayer("Obstacle")))
                {
                    //Debug.Log("Detect Obstacles");
                    node.isValid = false;
                }
            }
            //add neighbors
            var nodeList = AllNodes.Values.ToList(); // Convert to list for indexing
                                                     // Dictionary to store nodes by grid coordinates
            Coordinates = new Dictionary<(int, int), Node>();

            // Step 1: Assign nodes to grid cells
            foreach (var node in nodeList)
            {
                // gird origin offset
                Vector3 gridPos = node.position - _Grid.Origin;
                int xVal = Mathf.FloorToInt(gridPos.x / _Grid.Size);
                int zVal = Mathf.FloorToInt(gridPos.z / _Grid.Size);
                var key = (xVal, zVal);
                Coordinates[key] = node;
            }

            // Step 2: Build neighbors
            foreach (var kvp in Coordinates)
            {
                var node = kvp.Value;
                int x = kvp.Key.Item1;
                int z = kvp.Key.Item2;

                // Check all 8 neighbors
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        if (dx == 0 && dz == 0) continue; // skip self

                        var neighborKey = (x + dx, z + dz);
                        if (Coordinates.ContainsKey(neighborKey))
                        {
                            var neighbor = Coordinates[neighborKey];
                            if (!neighbor.isValid) continue;
                            node.AddNeighbor(neighbor);
                        }
                    }
                }
            }
        }

        //helpers

       
        public Node GetClosestValidNode(Vector3 pos)
        {
            //use FloorToInt to match initialization
            Vector3 localPos = pos - _Grid.Origin;
            int x = Mathf.FloorToInt(localPos.x / _Grid.Size);
            int z = Mathf.FloorToInt(localPos.z / _Grid.Size);

            if (Coordinates.TryGetValue((x, z), out Node targetNode) && targetNode.isValid)
                return targetNode;

            // Spiral search to find the nearest valid node without gaps
            int maxSearch = Mathf.Max(Mathf.FloorToInt(_Grid.Width), Mathf.FloorToInt(_Grid.Length));
            for (int radius = 1; radius <= maxSearch; radius++)
            {
                for (int i = -radius; i <= radius; i++)
                {
                    // Check top and bottom rows of the current "square" radius
                    if (TryGetValid(x + i, z + radius, out Node n1)) return n1;
                    if (TryGetValid(x + i, z - radius, out Node n2)) return n2;
                    // Check left and right columns (excluding corners already checked)
                    if (i > -radius && i < radius)
                    {
                        if (TryGetValid(x + radius, z + i, out Node n3)) return n3;
                        if (TryGetValid(x - radius, z + i, out Node n4)) return n4;
                    }
                }
            }
            return null;
        }

        private bool TryGetValid(int x, int z, out Node node)
        {
            if (Coordinates.TryGetValue((x, z), out node))
            {
                return node.isValid;
            }
            return false;
        }
        
        public void ClearNodeData ()
        {
            foreach (Node node in AllNodes.Values)
            { 
                node.parent = null;
                node.gVal = float.MaxValue;
                node.hVal = float.MaxValue;
                node.fVal = float.MaxValue;
            }
        }

    }
}
