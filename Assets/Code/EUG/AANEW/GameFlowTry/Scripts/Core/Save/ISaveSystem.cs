using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nobeta.EUG.GameFlowTry.Core.Save
{
    public interface ISaveSystem
    {
        bool HasSave { get; }
        void Save(SaveData data);
        SaveData Load();
        void Delete();
    }
}
