// ============================================================================
// Core Services Implementation - 核心服務實現
// 提供接口的具體實現，展示如何實現各個服務
// ============================================================================

using GDCO.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GDCO.Core.Services
{
    // ========================================================================
    // GameStateManager - 遊戲狀態管理實現
    // ========================================================================
    
    /// <summary>
    /// 遊戲狀態管理器
    /// 使用簡單的狀態機管理遊戲整體狀態
    /// </summary>
    public class GameStateManager : IGameStateManager
    {
        private GameState currentState = GameState.None;
        
        public GameState CurrentState => currentState;
        
        public event Action<GameState, GameState> OnStateChanged;
        
        public void ChangeState(GameState newState)
        {
            if (currentState == newState) return;
            
            GameState previousState = currentState;
            currentState = newState;
            
            Debug.Log($"遊戲狀態改變：{previousState} → {newState}");
            
            OnStateChanged?.Invoke(previousState, newState);
        }
    }  
}
