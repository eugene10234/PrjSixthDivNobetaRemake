// ============================================================================
// GameMaster - 遊戲總控制器
// 負責協調所有核心服務，管理遊戲生命週期
// 遵循 SOLID 原則，保持簡潔和可擴展性
// ============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using GDCO.Core.Interfaces;
using GDCO.Core.Services;

namespace GDCO.Core.Events
{
    // ========================================================================
    // 數據結構
    // ========================================================================
    
    [Serializable]
    public class GameStateData
    {
        public DateTime lastPlayTime;
        public float totalPlayTime;
    }
    
    [Serializable]
    public class GameSaveData
    {
        public string sceneName;
        public Vector3 playerPosition;
        public int playerHealth;
        public int playerLevel;
        // 更多存檔數據...
    }
    
    // ========================================================================
    // 遊戲事件
    // ========================================================================
    
    public struct GameInitializedEvent : IGameEvent { }
    public struct GameShutdownEvent : IGameEvent { }
    public struct NewGameStartedEvent : IGameEvent { }
    public struct GameLoadedEvent : IGameEvent 
    { 
        public GameSaveData SaveData; 
    }
    public struct GamePausedEvent : IGameEvent { }
    public struct GameResumedEvent : IGameEvent { }
}
