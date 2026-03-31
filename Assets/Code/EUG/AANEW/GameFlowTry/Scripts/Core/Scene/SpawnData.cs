using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nobeta.EUG.GameFlowTry.Core.Scene
{
    /// <summary>
    /// GameManager → GameController 的出生點傳遞物件。
    ///
    /// UseSceneDefault = true  → 忽略 Position/Rotation，
    ///                           GameController 直接用 Scene 裡的 SceneSpawnPoint。
    /// UseSceneDefault = false → 使用 Position/Rotation（來自存檔）。
    /// </summary>
    public class SpawnData
    {
        public string SceneName { get; set; }
        public bool UseSceneDefault { get; set; } // ✅ 新增
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; } // ✅ 新增
        public bool PlayIntroCutscene { get; set; }
    }
}
