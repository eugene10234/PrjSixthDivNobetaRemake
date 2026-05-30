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
    // AudioService - 音頻服務實現（骨架）
    // ========================================================================
    
    /// <summary>
    /// 音頻服務 - 管理音樂和音效
    /// 這是一個基礎實現，實際項目中可以使用 FMOD 或 Wwise
    /// </summary>
    public class AudioService : IAudioService, IInitializeAsync
    {
        private AudioSource musicSource;
        private List<AudioSource> sfxSources = new List<AudioSource>();
        
        private float masterVolume = 1f;
        private float musicVolume = 1f;
        private float sfxVolume = 1f;
        
        public bool IsInitialized { get; private set; }
        
        public float MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = Mathf.Clamp01(value);
                UpdateVolumes();
            }
        }
        
        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                UpdateMusicVolume();
            }
        }
        
        public float SFXVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
            }
        }
        
        public async Task InitializeAsync()
        {
            if (IsInitialized) return;
            
            // 創建音頻管理器物件
            GameObject audioManager = new GameObject("AudioManager");
            GameObject.DontDestroyOnLoad(audioManager);
            
            // 創建音樂源
            musicSource = audioManager.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            
            IsInitialized = true;
            
            await Task.CompletedTask;
        }
        
        public void PlayMusic(string musicName, bool loop = true, float fadeInDuration = 0f)
        {
            // 實際實現中需要從 Resources 或 Addressables 載入 AudioClip
            Debug.Log($"播放音樂：{musicName}");
            
            // AudioClip clip = Resources.Load<AudioClip>($"Audio/Music/{musicName}");
            // musicSource.clip = clip;
            // musicSource.loop = loop;
            // musicSource.Play();
        }
        
        public void StopMusic(float fadeOutDuration = 0f)
        {
            musicSource?.Stop();
        }
        
        public void PlaySFX(string sfxName, float volume = 1f)
        {
            // 實際實現中需要從音效池中獲取 AudioSource
            Debug.Log($"播放音效：{sfxName}");
        }
        
        public void PlaySFXAtPosition(string sfxName, Vector3 position, float volume = 1f)
        {
            Debug.Log($"在位置 {position} 播放音效：{sfxName}");
        }
        
        public void PauseAll()
        {
            musicSource?.Pause();
            foreach (var source in sfxSources)
            {
                source?.Pause();
            }
        }
        
        public void ResumeAll()
        {
            musicSource?.UnPause();
            foreach (var source in sfxSources)
            {
                source?.UnPause();
            }
        }
        
        private void UpdateVolumes()
        {
            UpdateMusicVolume();
        }
        
        private void UpdateMusicVolume()
        {
            if (musicSource != null)
            {
                musicSource.volume = masterVolume * musicVolume;
            }
        }
    }
}
