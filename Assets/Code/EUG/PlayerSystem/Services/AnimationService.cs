using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Interfaces;
using System;
using UnityEngine;
using VContainer;
// ========================================================================
// AnimationService - 動畫服務
// ========================================================================
namespace Nobeta.EUG.FSM.Services
{
    public class AnimationService : IAnimationService
    {
        private readonly Animator animator;

        [Inject]
        public AnimationService(Animator animator)
        {
            this.animator = animator;
        }

        public void PlayAnimation(string animationName, float crossFadeDuration = 0.1f)
        {
            if (animator == null)
            {
                Debug.LogWarning("Animator is null!");
                return;
            }

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
