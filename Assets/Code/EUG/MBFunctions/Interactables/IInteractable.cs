using System.Threading;
using Assets.Code.EUG.MBFunctions.GUI;
using Cysharp.Threading.Tasks;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.MBFunctions.PlayerUI;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Nobeta.EUG.Interfaces
{
    /// <summary>
    /// 可互動物體接口
    /// </summary>
    public interface IInteractable
    {
        string InteractableName { get; }
        float InteractionDuration { get; }
        void OnInteract();
    }
}
