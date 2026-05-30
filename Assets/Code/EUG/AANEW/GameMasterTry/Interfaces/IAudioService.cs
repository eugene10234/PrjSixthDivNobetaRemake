// ============================================================================
// Core Interfaces - 核心接口定義
// 定義所有服務的契約，遵循 SOLID 的 DIP（依賴倒置原則）
// ============================================================================

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GDCO.Core.Interfaces
{
    // ========================================================================
    // 音頻系統接口
    // ========================================================================
    
    /// <summary>
    /// 音頻服務接口
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// 主音量
        /// </summary>
        float MasterVolume { get; set; }
        
        /// <summary>
        /// 音樂音量
        /// </summary>
        float MusicVolume { get; set; }
        
        /// <summary>
        /// 音效音量
        /// </summary>
        float SFXVolume { get; set; }
        
        /// <summary>
        /// 播放音樂
        /// </summary>
        void PlayMusic(string musicName, bool loop = true, float fadeInDuration = 0f);
        
        /// <summary>
        /// 停止音樂
        /// </summary>
        void StopMusic(float fadeOutDuration = 0f);
        
        /// <summary>
        /// 播放音效
        /// </summary>
        void PlaySFX(string sfxName, float volume = 1f);
        
        /// <summary>
        /// 播放一次性音效（在指定位置）
        /// </summary>
        void PlaySFXAtPosition(string sfxName, Vector3 position, float volume = 1f);
        
        /// <summary>
        /// 暫停所有音頻
        /// </summary>
        void PauseAll();
        
        /// <summary>
        /// 恢復所有音頻
        /// </summary>
        void ResumeAll();
    }
}
