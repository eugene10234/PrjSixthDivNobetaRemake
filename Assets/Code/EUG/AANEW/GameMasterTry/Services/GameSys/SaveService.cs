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
    // SaveService - 存檔服務實現（簡化版）
    // ========================================================================
    
    /// <summary>
    /// 存檔服務 - 處理數據持久化
    /// 這裡提供 PlayerPrefs 的簡化實現，實際項目可替換為更強大的方案
    /// </summary>
    public class SaveService : ISaveService, IInitializeAsync
    {
        private const string SavePrefix = "SaveData_";
        
        public bool IsInitialized { get; private set; }
        
        public event Action<string> OnSaveCompleted;
        public event Action<string> OnLoadCompleted;
        
        public async Task InitializeAsync()
        {
            if (IsInitialized) return;
            
            // 初始化存檔系統（檢查權限、創建目錄等）
            IsInitialized = true;
            
            await Task.CompletedTask;
        }
        
        public async Task SaveAsync<T>(string key, T data) where T : class
        {
            string fullKey = SavePrefix + key;
            string json = JsonUtility.ToJson(data);
            
            PlayerPrefs.SetString(fullKey, json);
            PlayerPrefs.Save();
            
            OnSaveCompleted?.Invoke(key);
            
            await Task.CompletedTask;
        }
        
        public async Task<T> LoadAsync<T>(string key) where T : class
        {
            string fullKey = SavePrefix + key;
            
            if (!PlayerPrefs.HasKey(fullKey))
            {
                return null;
            }
            
            string json = PlayerPrefs.GetString(fullKey);
            T data = JsonUtility.FromJson<T>(json);
            
            OnLoadCompleted?.Invoke(key);
            
            await Task.CompletedTask;
            return data;
        }
        
        public async Task DeleteAsync(string key)
        {
            string fullKey = SavePrefix + key;
            PlayerPrefs.DeleteKey(fullKey);
            PlayerPrefs.Save();
            
            await Task.CompletedTask;
        }
        
        public async Task<bool> ExistsAsync(string key)
        {
            string fullKey = SavePrefix + key;
            await Task.CompletedTask;
            return PlayerPrefs.HasKey(fullKey);
        }
        
        public async Task ClearAllAsync()
        {
            // 清除所有帶前綴的存檔
            // 注意：這裡簡化處理，實際可能需要更精確的邏輯
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            
            await Task.CompletedTask;
        }
    }
}
