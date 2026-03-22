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
        }
        public List<Vector3> CalculatePath(Vector3 start, Vector3 end)
        {
            List<Node> Openlist = new List<Node>();
            List<Node> ClosedList = new List<Node>();
            _nodeData.ClearNodeData();
            Node startNode = _nodeData.GetClosestValidNode(start);
            Node endNode = _nodeData.GetClosestValidNode(end);

            if (startNode == null || endNode == null)
            {
                Debug.Log("start node or end node not found");
                return null;
            }

            startNode.gVal = 0;
            startNode.hVal = Vector3.Distance(startNode.position, endNode.position);
            startNode.fVal = startNode.gVal + startNode.hVal;
            startNode.parent = null;

            Openlist.Add(startNode);

            bool pathFound = false;

            while (Openlist.Count > 0)
            {
                Node currentNode = Openlist[0];

                for (int i = 1; i < Openlist.Count; i++)
                {
                    if (Openlist[i].fVal < currentNode.fVal ||
                       (Openlist[i].fVal == currentNode.fVal &&
                        Openlist[i].hVal < currentNode.hVal))
                    {
                        currentNode = Openlist[i];
                    }
                }

                if (currentNode.position == endNode.position)
                {
                    pathFound = true;
                    break;
                }

                Openlist.Remove(currentNode);
                ClosedList.Add(currentNode);

                foreach (Node node in currentNode.Neighbors)
                {
                    if (!node.isValid || ClosedList.Contains(node))
                        continue;
                    float moveCost = Vector3.Distance(currentNode.position, node.position);
                    float penalty = node.penalty;
                    float tentativeG = currentNode.gVal + moveCost + penalty;

                    if (!Openlist.Contains(node) || tentativeG < node.gVal)
                    {
                        node.parent = currentNode;
                        node.gVal = tentativeG;
                        node.hVal = Vector3.Distance(node.position, endNode.position);
                        node.fVal = node.gVal + node.hVal;

                        if (!Openlist.Contains(node))
                            Openlist.Add(node);
                    }
                }
            }
            if (!pathFound)
                return null;
            return ReconstructPath(endNode);
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
