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
    // SceneService - 場景管理服務實現
    // ========================================================================
    
    /// <summary>
    /// 場景服務 - 處理場景載入/卸載
    /// 包含載入進度追蹤和事件通知
    /// </summary>
    public class SceneServiceSimp : ISceneService, IInitializeAsync
    {
        private Scene currentScene;
        private bool isLoading;
        private float loadingProgress;
        
        public Scene CurrentScene => currentScene;
        public bool IsLoading => isLoading;
        public float LoadingProgress => loadingProgress;
        public bool IsInitialized { get; private set; }
        
        public event Action<string> OnSceneLoadStarted;
        public event Action<Scene> OnSceneLoadCompleted;
        public event Action<float> OnLoadingProgress;
        
        public async Task InitializeAsync()
        {
            if (IsInitialized) return;
            
            currentScene = SceneManager.GetActiveScene();
            IsInitialized = true;
            
            await Task.CompletedTask;
        }
        
        public async Task<Scene> LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (isLoading)
            {
                Debug.LogWarning($"場景載入中，無法載入新場景：{sceneName}");
                return default;
            }
            
            isLoading = true;
            loadingProgress = 0f;
            
            OnSceneLoadStarted?.Invoke(sceneName);
            
            try
            {
                AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);
                
                while (!operation.isDone)
                {
                    loadingProgress = operation.progress;
                    OnLoadingProgress?.Invoke(loadingProgress);
                    await Task.Yield();
                }
                
                currentScene = SceneManager.GetSceneByName(sceneName);
                loadingProgress = 1f;
                
                OnSceneLoadCompleted?.Invoke(currentScene);
                
                return currentScene;
            }
            finally
            {
                isLoading = false;
            }
        }
        
        public async Task UnloadSceneAsync(string sceneName)
        {
            AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);
            
            while (!operation.isDone)
            {
                await Task.Yield();
            }
        }
    }   
}