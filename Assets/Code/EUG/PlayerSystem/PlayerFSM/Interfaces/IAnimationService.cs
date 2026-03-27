// ========================================================================
// Animation Service - 動畫服務接口
// ========================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nobeta.EUG.FSM.Interfaces
{
    /// <summary>
    /// 動畫服務接口 - 處理角色動畫
    /// </summary>
    public interface IAnimationService
    {
        /// <summary>
        /// 播放動畫
        /// </summary>
        void PlayAnimation(string animationName, float crossFadeDuration = 0.1f, bool force = false);

        /// <summary>
        /// 設置動畫參數（浮點）
        /// </summary>
        void SetFloat(string parameterName, float value);

        /// <summary>
        /// 設置動畫參數（布林）
        /// </summary>
        void SetBool(string parameterName, bool value);

        /// <summary>
        /// 設置動畫參數（觸發器）
        /// </summary>
        void SetTrigger(string parameterName);
        bool IsInTransition(int v);
        Animator GetAnimator { get; }
        AnimationClip GetClip(string name);
        Dictionary<string, AnimationClip> GetClipCache { get; }
    }
}


