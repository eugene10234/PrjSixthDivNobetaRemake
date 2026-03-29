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
        public float PosX, PosY, PosZ;

        public Vector3 PlayerPosition => new(PosX, PosY, PosZ);

        public void SetPlayerPosition(Vector3 pos)
        {
            PosX = pos.x; PosY = pos.y; PosZ = pos.z;
        }
    }
}
