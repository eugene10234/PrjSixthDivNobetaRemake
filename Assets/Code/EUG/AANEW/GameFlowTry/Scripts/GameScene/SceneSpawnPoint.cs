using UnityEngine;

namespace Nobeta.EUG.GameFlowTry.GameScene
{
    public class SceneSpawnPoint : MonoBehaviour
    {
        private Vector3 _position;
        private Quaternion _rotation;
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        private void Awake()
        {
            Position = transform.position;
            Rotation = transform.rotation;
        }
    }
}