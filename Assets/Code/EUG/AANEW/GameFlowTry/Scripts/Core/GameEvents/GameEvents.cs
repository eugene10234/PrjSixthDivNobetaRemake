using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nobeta.EUG.GameFlowTry.Core.GameEvents
{
    public readonly struct NewGameRequestedEvent { }
    public readonly struct ContinueGameRequestedEvent { }
    public readonly struct ReturnToMenuRequestedEvent { }
    public readonly struct GameSavedEvent { }

    // ✅ Rotation 一併存入
    public readonly struct SaveGameRequestedEvent
    {
        public readonly string SceneName;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation; // ✅ 新增

        public SaveGameRequestedEvent(string sceneName, Vector3 position, Quaternion rotation)
        {
            SceneName = sceneName;
            Position = position;
            Rotation = rotation; // ✅
        }
    }

    public readonly struct SceneLoadStartedEvent
    {
        public readonly string SceneName;
        public SceneLoadStartedEvent(string s) => SceneName = s;
    }

    public readonly struct SceneLoadProgressEvent
    {
        public readonly float Progress;
        public SceneLoadProgressEvent(float p) => Progress = p;
    }

    public readonly struct SceneLoadCompletedEvent
    {
        public readonly string SceneName;
        public SceneLoadCompletedEvent(string s) => SceneName = s;
    }

    public readonly struct CutsceneStartedEvent
    {
        public readonly string CutsceneId;
        public CutsceneStartedEvent(string id) => CutsceneId = id;
    }

    public readonly struct CutsceneCompletedEvent
    {
        public readonly string CutsceneId;
        public CutsceneCompletedEvent(string id) => CutsceneId = id;
    }
    public readonly struct LoadSceneRequestedEvent
    {
        public readonly string SceneName;
        public LoadSceneRequestedEvent(string sceneName) => SceneName = sceneName;
    }
}
