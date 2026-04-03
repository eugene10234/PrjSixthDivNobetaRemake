using Cysharp.Threading.Tasks;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.Interfaces;
using System;
using System.Threading;
using UnityEngine;
using VContainer.Unity;
using static UnityEngine.Application;
// ========================================================================
// TimeService - 時間服務
// ========================================================================
namespace Nobeta.EUG.Services
{
    public class TimeService : ITimeService, ITickable
    {
        private float gameTime;
        private bool isPaused;

        public float GameTime => gameTime;
        public float DeltaTime => Time.deltaTime;
        public float FixedDeltaTime => Time.fixedDeltaTime;
        public bool IsPaused => isPaused;

        public void UpdateGameTime()
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
        public event Action SecondPassed;
        public int SecondsPassed { get; private set; }

        private float _elapsedTime;

        public void Tick()
        {
            UpdateGameTime();
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime < 1f)
            {
                return;
            }

            _elapsedTime = 0f;
            SecondsPassed++;
            SecondPassed?.Invoke();
        }


        /// <summary>
        /// 等待指定秒數，並在等待期間將傳入的 bool 設為 true (鎖定)
        /// </summary>
        /// <param name="seconds">等待秒數</param>
        /// <param name="lockCallback">用來修改鎖定狀態的委託</param>
        public async UniTask TaskDelayLock(float seconds, Action<bool> lockCallback, CancellationToken cancellationToken = default)
        {
            lockCallback?.Invoke(true);
            try
            {
                // 使用 UniTask 並傳入取消令牌，這樣當物件毀損時會自動停止等待
                await UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // 被取消時的處理（可選）
            }
            finally
            {
                lockCallback?.Invoke(false);
            }
        }
        public async UniTaskVoid DelayAction(Action DelayedAction, float duration, CancellationToken cancellationToken = default)
        {
            try
            {
                await UniTask.Delay((int)(1000 * duration), cancellationToken: cancellationToken);                
            }
            catch (System.OperationCanceledException) { }
            finally
            {
                DelayedAction?.Invoke();
            }
        }
    }
}
