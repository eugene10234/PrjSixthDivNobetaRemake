// ============================================================================
// Core Services Implementation - 核心服務實現
// 提供接口的具體實現，展示如何實現各個服務
// ============================================================================

using GDCO.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GDCO.Core 
{ 

    public class ServiceLifecycleManager:IDisposable
    {
        private bool _isInitialized = false;
        private List<IInitializeAsync> initializableServices = new List<IInitializeAsync>();
        private List<IUpdatable> updatableServices = new List<IUpdatable>();
        private List<IFixedUpdatable> fixedUpdatableServices = new List<IFixedUpdatable>();
        private List<IDisposableService> disposableServices = new List<IDisposableService>();
        public bool isInitialized { get => _isInitialized; }
        public void Initialized()
        {
            _isInitialized = true;
        }
        public async Task<bool> InitializeAsync()
        {
            foreach (var service in initializableServices)
            {
                if (service != null && !service.IsInitialized)
                {
                    await service.InitializeAsync();
                }
            }            
            return true;
        }
        public void OnUpdate()
        {
            if (!isInitialized) return;
            foreach (var service in updatableServices) service.Update();
        }

        public void OnFixedUpdate()
        {
            if (!isInitialized) return;
            foreach (var service in fixedUpdatableServices) service.FixedUpdate();
        }
        /// <summary>
        /// 綁定並初始化所有服務
        /// </summary>
        public void BindServices(IEnumerable<object> allServices)
        {
            // 從容器中獲取所有已註冊的服務
            //IEnumerable<object> allServices = container.Resolve<IEnumerable<object>>();

            foreach (var service in allServices)
            {
                if (service is IInitializeAsync initializable)
                    initializableServices.Add(initializable);

                if (service is IUpdatable updatable)
                    updatableServices.Add(updatable);

                if (service is IFixedUpdatable fixedUpdatable)
                    fixedUpdatableServices.Add(fixedUpdatable);

                if (service is IDisposableService disposable)
                    disposableServices.Add(disposable);
            }
        }
        /// <summary>
        /// 清理所有可清理的服務和列表
        /// </summary>
        public void Dispose()
        {
            foreach (var service in disposableServices)
            {
                service?.Cleanup();
                service?.Dispose();
            }

            initializableServices.Clear();
            updatableServices.Clear();
            fixedUpdatableServices.Clear();
            disposableServices.Clear();

            _isInitialized = false;
        }
    }

}