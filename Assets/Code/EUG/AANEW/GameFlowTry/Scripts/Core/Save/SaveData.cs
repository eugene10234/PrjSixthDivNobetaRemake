using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nobeta.EUG.GameFlowTry.Core.Save
{
    [Serializable]
    public class SaveData
    {
        public string SceneName;

        // Position
        public float PosX, PosY, PosZ;

        // Rotation（四元數，完整保留旋轉狀態）
        public float RotX, RotY, RotZ, RotW;

        public Vector3 PlayerPosition => new(PosX, PosY, PosZ);
        public Quaternion PlayerRotation => new(RotX, RotY, RotZ, RotW);

        public void SetPlayerTransform(Vector3 pos, Quaternion rot)
        {
            PosX = pos.x; PosY = pos.y; PosZ = pos.z;
            RotX = rot.x; RotY = rot.y; RotZ = rot.z; RotW = rot.w;
        }
    }
}
