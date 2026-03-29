using Cysharp.Threading.Tasks;
using Nobeta.EUG.GameFlowTry.Core.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nobeta.EUG.GameFlowTry.Game
{
    public interface IGameManager
    {
        SpawnData PendingSpawn { get; }
        UniTask StartNewGameAsync();
        UniTask ContinueGameAsync();
        UniTask ReturnToMenuAsync();
        UniTask SaveGameAsync(string sceneName, Vector3 playerPosition);
    }
}
