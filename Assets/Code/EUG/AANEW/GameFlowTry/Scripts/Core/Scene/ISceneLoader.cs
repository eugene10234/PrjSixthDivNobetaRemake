using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nobeta.EUG.GameFlowTry.Core.Scene
{
    public interface ISceneLoader
    {
        UniTask LoadSceneAsync(string sceneName);
    }
}
