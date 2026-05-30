using GDCO.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GDCO.Core.Services
{
    public class InputService : IInputService
    {
        public bool InputEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool GetButton(string buttonName)
        {
            throw new NotImplementedException();
        }

        public bool GetButtonDown(string buttonName)
        {
            throw new NotImplementedException();
        }

        public bool GetButtonUp(string buttonName)
        {
            throw new NotImplementedException();
        }

        public Vector2 GetLookInput()
        {
            throw new NotImplementedException();
        }

        public Vector2 GetMovementInput()
        {
            throw new NotImplementedException();
        }

        public void SetInputMode(InputMode mode)
        {
            throw new NotImplementedException();
        }
    }
    public class ResourceService : IResourceService
    {
        public Task<GameObject> InstantiateAsync(string path, Transform parent = null)
        {
            throw new NotImplementedException();
        }

        public Task<T[]> LoadAllAsync<T>(string path) where T : UnityEngine.Object
        {
            throw new NotImplementedException();
        }

        public Task<T> LoadAsync<T>(string path) where T : UnityEngine.Object
        {
            throw new NotImplementedException();
        }

        public void Release(UnityEngine.Object asset)
        {
            throw new NotImplementedException();
        }

        public void ReleaseAll()
        {
            throw new NotImplementedException();
        }
    }
    public class UIService : IUIService
    {
        public T GetPanel<T>() where T : UIPanel
        {
            throw new NotImplementedException();
        }

        public Task HidePanelAsync<T>() where T : UIPanel
        {
            throw new NotImplementedException();
        }

        public bool IsPanelShown<T>() where T : UIPanel
        {
            throw new NotImplementedException();
        }

        public Task<bool> ShowConfirmDialogAsync(string title, string message)
        {
            throw new NotImplementedException();
        }

        public void ShowNotification(string message, float duration = 3)
        {
            throw new NotImplementedException();
        }

        public Task ShowPanelAsync<T>(object data = null) where T : UIPanel
        {
            throw new NotImplementedException();
        }
    }
}
