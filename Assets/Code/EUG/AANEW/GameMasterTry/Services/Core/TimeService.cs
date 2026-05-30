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
    // TimeService - 時間服務實現
    // ========================================================================
    
    /// <summary>
    /// 時間服務 - 管理遊戲時間和暫停
    /// </summary>
    public class TimeService : ITimeService, IUpdatable
    {
        private float timeScale = 1f;
        private float gameTime = 0f;
        private float realTime = 0f;
        private bool isPaused = false;
        
        public float TimeScale
        {
            get => timeScale;
            set
            {
                timeScale = Mathf.Max(0f, value);
                Time.timeScale = isPaused ? 0f : timeScale;
            }
        }
        
        public float GameTime => gameTime;
        public float RealTime => realTime;
        public float DeltaTime => Time.deltaTime;
        public bool IsPaused => isPaused;
        
        public void Update()
        {
            if (!isPaused)
            {
                gameTime += Time.deltaTime;
            }
            realTime += Time.unscaledDeltaTime;
        }
        
        public void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;
        }
        
        public void Resume()
        {
            isPaused = false;
            Time.timeScale = timeScale;
        }
    }
}
