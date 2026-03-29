using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nobeta.EUG.GameFlowTry.Core.Scene
{
    public class SpawnData
    {
        public string SceneName { get; set; }
        public Vector3 Position { get; set; }
        public bool PlayIntroCutscene { get; set; }
    }
}
