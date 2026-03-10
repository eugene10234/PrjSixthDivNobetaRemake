using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Interfaces;
using System;
using UnityEngine;
using VContainer;
using static UnityEngine.UI.GridLayoutGroup;
// ========================================================================
// AnimationService - 動畫服務
// ========================================================================
namespace Nobeta.EUG.FSM.Services
{
    public class AnimationService : IAnimationService
    {
        private readonly Animator animator;
        private AnimatorStateInfo _LatetestStateInfo;

        //_stateHash = Animator.StringToHash("SpawnAnimation");
        [Inject]
        public AnimationService(Animator animator)
        {
            this.animator = animator;

        }
        public bool IsInTransition(int layerId) 
        {
            return animator != null && animator.IsInTransition(layerId);
        }
        public void PlayAnimation(string animationName, float crossFadeDuration = 0.1f , bool force = false)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            if (animator == null)
            {
                Debug.LogWarning("Animator is null!");
                return;
            }
            //if (stateInfo.normalizedTime >= 1.0f) _LatetestStateInfo = stateInfo;
            if (stateInfo.IsName(animationName) )return; // 已經在播放目標動畫，避免重複觸發
            if (animator.IsInTransition(0) && force)
            {
                animator.Play(animationName, 0, 0f);
            }
            _LatetestStateInfo = stateInfo;
            //Debug.Log($"Playing animation: {animationName}, Current state: {_LatetestStateInfo.fullPathHash}, Normalized time: {_LatetestStateInfo.normalizedTime}");
            animator.CrossFade(animationName, crossFadeDuration);

        }

        public void SetFloat(string parameterName, float value)
        {
            if (animator == null) return;
            animator.SetFloat(parameterName, value, .15f, Time.deltaTime);
        }

        public void SetBool(string parameterName, bool value)
        {
            if (animator == null) return;
            animator.SetBool(parameterName, value);
        }

        public void SetTrigger(string parameterName)
        {
            if (animator == null) return;
            animator.SetTrigger(parameterName);
        }
    }
}
