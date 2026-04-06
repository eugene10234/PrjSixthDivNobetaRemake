using Nobeta.EUG.Interfaces;
using Nobeta.EUG.MBFunctions.Interacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nobeta.EUG.Events
{
    public struct ChaseAimStartEvent
    {
        public Vector3 TargetPosition;
        public Vector3 TargetNormal;
    }
}
