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
    // ConfigService - 配置服務實現
    // ========================================================================
    
    /// <summary>
    /// 配置服務 - 管理 ScriptableObject 配置
    /// </summary>
    public class ConfigService : IConfigService
    {
        private readonly Dictionary<Type, ScriptableObject> configs = new Dictionary<Type, ScriptableObject>();
        
        public T GetConfig<T>() where T : ScriptableObject
        {
            Type type = typeof(T);
            
            if (configs.TryGetValue(type, out ScriptableObject config))
            {
                return config as T;
            }
            
            Debug.LogWarning($"配置未找到：{type.Name}");
            return null;
        }
        
        public async Task LoadConfigAsync<T>(string path) where T : ScriptableObject
        {
            T config = Resources.Load<T>(path);
            
            if (config != null)
            {
                configs[typeof(T)] = config;
            }
            else
            {
                Debug.LogError($"載入配置失敗：{path}");
            }
            
            await Task.CompletedTask;
        }
        
        public bool HasConfig<T>() where T : ScriptableObject
        {
            return configs.ContainsKey(typeof(T));
        }
    }
}
