using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Pathfinding
{
    public class GridVisual : MonoBehaviour
    {

        private NodeData _nodeData;
        public float radius = 0.2f;
        [Inject]
        public void Construct(NodeData nm)
        { 
            _nodeData = nm;
            Debug.Log($"<color=green>Successfully injected NodeData into {gameObject.name}</color>");
        }

        private void OnDrawGizmos()
        {
            if (_nodeData == null) return;
            foreach (var kvp in _nodeData.AllNodes)
            {
                Node node = kvp.Value;
                if (node.isValid == false) { Gizmos.color = Color.red; }
                else { Gizmos.color = Color.green; }
                Gizmos.DrawSphere(node.position, radius);
            }
        }
    }
}
