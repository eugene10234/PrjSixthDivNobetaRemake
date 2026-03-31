using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nobeta.EUG.GameFlowTry.Menu
{
    /// <summary>純 View，不含任何遊戲邏輯。</summary>
    public class MenuView : MonoBehaviour
    {
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private TextMeshProUGUI _statusText;

        public event Action OnNewGameClicked;
        public event Action OnContinueClicked;

        private void Awake()
        {
            _newGameButton.onClick.AddListener(() => OnNewGameClicked?.Invoke());
            _continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
        }

        public void SetContinueInteractable(bool v) => _continueButton.interactable = v;
        public void ShowStatus(string msg) { if (_statusText) _statusText.text = msg; }
    }
}
