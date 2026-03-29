using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nobeta.EUG.GameFlowTry.GameScene
{
    public class GameHudView : MonoBehaviour
    {
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private TextMeshProUGUI _statusText;

        public event Action OnSaveClicked;
        public event Action OnMenuClicked;

        private void Awake()
        {
            _saveButton.onClick.AddListener(() => OnSaveClicked?.Invoke());
            _menuButton.onClick.AddListener(() => OnMenuClicked?.Invoke());
        }

        public void ShowStatus(string msg) { if (_statusText) _statusText.text = msg; }
    }
}
