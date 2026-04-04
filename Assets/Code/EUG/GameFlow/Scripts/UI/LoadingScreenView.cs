using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nobeta.EUG.GameFlow.UI
{
    /// <summary>
    /// 載入遮罩 Canvas（DontDestroyOnLoad）。
    /// 場景切換全程遮擋，SetProgress() 由 SceneLoader 驅動。
    /// </summary>
    public class LoadingScreenView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        //[SerializeField] private Slider _progressBar;
        [SerializeField] private Image _progressBar;
        [SerializeField] private TextMeshProUGUI _progressText;

        private void Awake() => _root.SetActive(false);

        public void Show() { _root.SetActive(true); SetProgress(0f); }
        public void Hide() { _root.SetActive(false); }

        public void SetProgress(float value)
        {
            //if (_progressBar != null) _progressBar.value = value;
            if (_progressBar != null) _progressBar.fillAmount = value;
            if (_progressText != null) _progressText.text = $"讀取中...{(int)(value * 100)}%";
        }
    }
}
