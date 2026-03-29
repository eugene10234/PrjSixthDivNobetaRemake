using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nobeta.EUG.GameFlowTry.Constants
{
    public static class SceneNames
    {
        public const string Menu = "MenuScene";
        public const string Game1 = "GameScene1";
        public const string Game2 = "GameScene2";
    }

    public static class SpawnPoints
    {
        public static readonly UnityEngine.Vector3 Game1Default = new(0f, 1f, 0f);
        public static readonly UnityEngine.Vector3 Game2Default = new(5f, 1f, 0f);
    }
}
