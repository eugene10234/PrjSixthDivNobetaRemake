using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nobeta.EUG.GameFlowTry.GameScene
{
    /// <summary>
    /// 玩家移動，DrawDebugInfo() 視覺化位置、速度向量、Spawn 點。
    /// </summary>
    public class PlayerTeleporter : MonoBehaviour
    {
        private CharacterController _cc;
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation; // ✅ 新增
        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _cc.enabled = false;
            transform.position = position;
            transform.rotation = rotation; // ✅
            _cc.enabled = true;
        }
    }
}
