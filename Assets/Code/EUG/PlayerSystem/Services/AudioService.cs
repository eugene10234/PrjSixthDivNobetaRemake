// ============================================================================
// Additional Services Implementation - 其他服務實現
// ============================================================================

using Nobeta.EUG.FSM.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Nobeta.EUG.FSM.Services
{
    // ========================================================================
    // AudioService - 音頻服務（簡化版）
    // ========================================================================

    public class AudioService : IAudioService
    {
        private AudioSource musicSource;
        private readonly List<AudioSource> sfxPool = new List<AudioSource>();

        public void PlaySFX(string sfxName, float volume = 1f)
        {
            // 簡化實現：實際項目中從資源系統載入
            Debug.Log($"Playing SFX: {sfxName} at volume {volume}");

            // AudioClip clip = Resources.Load<AudioClip>($"Audio/SFX/{sfxName}");
            // AudioSource.PlayClipAtPoint(clip, Vector3.zero, volume);Assets/MasterMagicFX/ParticlesVer3/Cosmos/CosmoShoot2/Prefabs/Par_CosmoShootMuzzle.prefab
        }

        public void PlayMusic(string musicName, bool loop = true)
        {
            Debug.Log($"Playing Music: {musicName}");

            // 實際實現中設置音樂源
        }

        public void StopMusic(float fadeOutDuration = 0f)
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }
    }
}