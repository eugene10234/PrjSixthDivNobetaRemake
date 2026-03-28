using System.Collections.Generic;
using UnityEngine;

namespace Nobeta.Sam.Pathfinding
{
    public class Node
    {
        public float gVal;
        public float hVal;
        public float fVal;

        public int enemiesOccupied;

        public float penalty => enemiesOccupied * enemiesOccupied * enemyPenaltyRate;

        public Node parent;
        public bool isValid { get; set; }
        public Vector3 position { get; }

        public List<Node> Neighbors { get; } = new List<Node>();

        private float enemyPenaltyRate;

        public Node(Vector3 position, float enemyPenalty)
        {
            this.position = position;
            this.enemyPenaltyRate = enemyPenalty;

            isValid = true;
            parent = null;

            gVal = float.MaxValue;
            hVal = float.MaxValue;
            fVal = float.MaxValue;
        }

        public void AddEnemy() => enemiesOccupied++;

        public void RemoveEnemy()
        {
            enemiesOccupied = Mathf.Max(0, enemiesOccupied - 1);
        }

        public void AddNeighbor(Node nNode)
        {
            if (!Neighbors.Contains(nNode))
                Neighbors.Add(nNode);
        }
    }
}