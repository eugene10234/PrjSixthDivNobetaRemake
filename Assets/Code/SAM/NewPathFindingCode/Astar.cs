using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Pathfinding
{
    public class Astar : IPathFindingStrat
    {
        
        private NodeData _nodeData;

        [Inject]
        public void Construct(NodeData nodedata)
        {
            _nodeData = nodedata;
            Debug.Log("Astar is Ready to Function");
        }
        public List<Vector3> CalculatePath(Vector3 start, Vector3 end)
        {
            // Use PriorityQueue for O(log n) min extraction
            var openQueue = new MinHeap<Node>();
            var openSet = new HashSet<Node>();       // for O(1) membership check
            var closedSet = new HashSet<Node>();     // replaces List<Node> ClosedList
            var dirtyNodes = new List<Node>();       // track what to reset later

            Node startNode = _nodeData.GetClosestValidNode(start);
            Node endNode = _nodeData.GetClosestValidNode(end);

            if (startNode == null || endNode == null) return null;

            startNode.gVal = 0;
            startNode.hVal = Vector3.Distance(startNode.position, endNode.position);
            startNode.fVal = startNode.hVal;
            startNode.parent = null;

            openQueue.Enqueue(startNode, startNode.fVal);
            openSet.Add(startNode);
            dirtyNodes.Add(startNode);

            bool pathFound = false;

            while (openQueue.Count > 0)
            {
                Node current = openQueue.Dequeue();
                openSet.Remove(current);

                if (current.position == endNode.position)
                {
                    pathFound = true;
                    break;
                }

                closedSet.Add(current);

                foreach (Node neighbor in current.Neighbors)
                {
                    if (!neighbor.isValid || closedSet.Contains(neighbor)) continue;

                    float tentativeG = current.gVal
                        + Vector3.Distance(current.position, neighbor.position)
                        + neighbor.penalty;

                    if (tentativeG < neighbor.gVal)
                    {
                        neighbor.parent = current;
                        neighbor.gVal = tentativeG;
                        neighbor.hVal = Vector3.Distance(neighbor.position, endNode.position);
                        neighbor.fVal = neighbor.gVal + neighbor.hVal;

                        if (!openSet.Contains(neighbor))
                        {
                            openQueue.Enqueue(neighbor, neighbor.fVal);
                            openSet.Add(neighbor);
                            dirtyNodes.Add(neighbor);
                        }
                        // Note: PriorityQueue doesn't support decrease-key.
                        // Re-enqueue with new priority; stale entries are harmless
                        // (they'll be skipped once the node is in closedSet).
                        else
                        {
                            openQueue.Enqueue(neighbor, neighbor.fVal);
                        }
                    }
                }
            }

            // Only reset nodes we actually touched
            foreach (Node n in dirtyNodes)
            {
                n.parent = null;
                n.gVal = float.MaxValue;
                n.hVal = float.MaxValue;
                n.fVal = float.MaxValue;
            }

            return pathFound ? ReconstructPath(endNode) : null;
        }
        public List<Vector3> ReconstructPath(Node endNode)
        {
            List<Vector3> path = new List<Vector3>();
            Node current = endNode;
            while (current != null)
            {
                path.Add(current.position);
                current = current.parent;
            }
            path.Reverse();
            return path;
        }
    }
}
