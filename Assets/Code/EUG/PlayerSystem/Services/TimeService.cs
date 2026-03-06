using Nobeta.EUG.FSM.Core;
using System;
using UnityEngine;
using Nobeta.EUG.FSM.Interfaces;
// ========================================================================
// TimeService - 時間服務
// ========================================================================
namespace Nobeta.EUG.FSM.Services
{
    public class TimeService : ITimeService
    {
        private float gameTime;
        private bool isPaused;

        public float GameTime => gameTime;
        public float DeltaTime => Time.deltaTime;
        public float FixedDeltaTime => Time.fixedDeltaTime;
        public bool IsPaused => isPaused;

        public void Update()
        {
            if (!isPaused)
            {
                gameTime += Time.deltaTime;
            }
        }

        public void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;
        }

        public void Resume()
        {
            isPaused = false;
            Time.timeScale = 1f;
        }
    }
}
