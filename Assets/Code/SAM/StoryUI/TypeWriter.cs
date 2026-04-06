using DG.Tweening;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.MBFunctions.PlayerUI;
using TMPro;
using UnityEngine;

namespace Nobeta.Sam.StoryUI
{
    public class Typewriter : MonoBehaviour
    {
        public TMP_Text text;
        public float secondsPerChar = 0.5f;
        private IEventBus EventBus;
        private Tween typingTween;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
            EventBus = EventBusProvider.GetEventBus();
        }

        public void HideText()
        {
            typingTween?.Kill();
            text.text = "";
            text.maxVisibleCharacters = 0;
        }

        public void FastShowText(string message)
        {
            typingTween?.Kill();

            text.text = message;
            text.maxVisibleCharacters = 0;
            text.ForceMeshUpdate();

            int totalChars = text.textInfo.characterCount;

            typingTween = DOTween.To(
                () => text.maxVisibleCharacters,
                x => text.maxVisibleCharacters = x,
                totalChars,
                totalChars * secondsPerChar * 0.25f  // 4x faster = 0.125s per char
            ).SetEase(Ease.Linear);
        }
        public void ShowText(string message)
        {
            typingTween?.Kill();

            text.text = message;
            text.maxVisibleCharacters = 0;
            text.ForceMeshUpdate();

            int totalChars = text.textInfo.characterCount;

            EventBus.Publish(new InteractEreaEnterEvent
            {
                Message = $"字數:{message.Length}"
            });

            typingTween = DOTween.To(
                () => text.maxVisibleCharacters,
                x => text.maxVisibleCharacters = x,
                totalChars,
                totalChars * secondsPerChar*0.5f
            ).SetEase(Ease.Linear);
        }

        public void DirectShowText(string message)
        {
            typingTween?.Kill();
            text.text = message;
            text.maxVisibleCharacters = int.MaxValue;
        }
    }
}
