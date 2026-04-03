using Assets.Code.EUG.MBFunctions.GUI;
using Cysharp.Threading.Tasks;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
// using TMPro; // 如果你有用到文字

namespace Nobeta.EUG.MBFunctions.PlayerUI
{
    public abstract class MageIconHolder : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image MageCDFilter;
        [SerializeField] private CanvasGroup MageIconSparker;
        private void Awake()
        {
            MageIconSparker.alpha = 0;
        }        
        public Image GetFilter { get => MageCDFilter; }
        public void SetActice(bool TF) 
        {
            this.gameObject.SetActive(TF);
        }
        public abstract Color GetOriginColor { get; }
        public CanvasGroup GetIconSparker { get => MageIconSparker; }
        //GameObject _gameObject;
        //private void Awake()
        //{
        //    _gameObject = GetComponent<GameObject>();
        //    _gameObject= this.gameObject;
        //}

    }
}
