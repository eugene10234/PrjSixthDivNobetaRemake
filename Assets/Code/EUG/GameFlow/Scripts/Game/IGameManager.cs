using Cysharp.Threading.Tasks;
using Nobeta.EUG.GameFlow.Core.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nobeta.EUG.GameFlow.Game
{
    public interface IGameManager
    {
        SpawnData PendingSpawn { get; }
        UniTask StartNewGameAsync();
        UniTask ContinueGameAsync();
        UniTask ReturnToMenuAsync();
        UniTask SaveGameAsync(string sceneName, Vector3 position, Quaternion rotation);
    }
}
