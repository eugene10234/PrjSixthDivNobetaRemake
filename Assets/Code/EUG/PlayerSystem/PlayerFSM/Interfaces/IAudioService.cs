// ========================================================================
// Audio Service - 音頻服務接口
// ========================================================================
using System;
using UnityEngine;

namespace Nobeta.EUG.FSM.Interfaces
{
    /// <summary>
    /// 音頻服務接口
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// 播放音效
        /// </summary>
        void PlaySFX(string sfxName, float volume = 1f);

        /// <summary>
        /// 播放音樂
        /// </summary>
        void PlayMusic(string musicName, bool loop = true);

        /// <summary>
        /// 停止音樂
        /// </summary>
        void StopMusic(float fadeOutDuration = 0f);
    }


}


