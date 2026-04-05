using UnityEngine;

namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public struct CollisionHit
    {
        public bool HasHit;
        public GameObject GameObject;
        public Vector3 Normal;

        public Vector3 Point { get; internal set; }
    }
}
