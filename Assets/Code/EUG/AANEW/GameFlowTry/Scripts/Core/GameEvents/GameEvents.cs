using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nobeta.EUG.GameFlowTry.Core.GameEvents
{
    public readonly struct NewGameRequestedEvent { }
    public readonly struct ContinueGameRequestedEvent { }
    public readonly struct ReturnToMenuRequestedEvent { }
    public readonly struct GameSavedEvent { }

    public readonly struct SaveGameRequestedEvent
    {
        public readonly string SceneName;
        public readonly UnityEngine.Vector3 Position;
        public SaveGameRequestedEvent(string sceneName, UnityEngine.Vector3 position)
        {
            SceneName = sceneName;
            Position = position;
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
}
