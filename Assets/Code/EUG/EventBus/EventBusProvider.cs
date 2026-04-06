// ========================================================================
// EventBusProvider - EventBus 提供者（單例）
// ========================================================================

using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using System;
using System.Collections.Generic;
using UnityEngine;

using VContainer;

/// <summary>
/// EventBus 提供者
/// 用於讓場景中的對象輕鬆訪問 EventBus
/// 
/// 放在場景的根物件上
/// </summary>
public class EventBusProvider : MonoBehaviour
{
    private static EventBusProvider instance;
    private IEventBus eventBus;

    public IEventBus EventBus => eventBus;

    [Inject]
    public void Construct(IEventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static IEventBus GetEventBus()
    {
        return instance?.eventBus;
    }
}
//public class GameProvider : MonoBehaviour
//{
//    private static GameProvider instance;
//    private IPlayerInputService playerInputService;

//    public IPlayerInputService PlayerInputService => playerInputService;

//    [Inject]
//    public void Construct(IPlayerInputService PlayerInputService)
//    {
//        this.playerInputService = PlayerInputService;
//    }

//    private void Awake()
//    {
//        if (instance == null)
//        {
//            instance = this;
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }
//    public static IPlayerInputService GetPlayerInputService()
//    {
//        return instance?.playerInputService;
//    }
//    public static T GetService<T>() where T : class
//    {
//        var type = typeof(T);

//        if (type == typeof(IPlayerInputService))
//        {
//            return instance?.playerInputService as T;
//        }

//        // 如果有其他服務
//        // if (type == typeof(IAudioService)) return instance?.audioService as T;

//        return null;
//    }
//}
//public class ServiceManager : MonoBehaviour
//{
//    private static ServiceManager instance;
//    // 使用字典存儲所有服務
//    private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

//    private void Awake()
//    {
//        instance = this;
//        // 初始化並註冊服務
//        var eventBus = new EventBus();
//        RegisterService<IEventBus>(eventBus);

//        var inputService = new PlayerInputService();
//        RegisterService<IPlayerInputService>(inputService);

//        // 2. 註冊需要依賴的服務，直接從已註冊的服務中取得
//        // 這裡就是你問的地方：將依賴項傳入建構子
//        RegisterService<ICameraService>(new CameraService(inputService, eventBus));
//    }

//    public void RegisterService<T>(T service) => _services[typeof(T)] = service;

//    public static T GetService<T>() where T : class
//    {
//        if (instance != null && instance._services.TryGetValue(typeof(T), out var service))
//        {
//            return service as T;
//        }
//        return null;
//    }
//}
//// 簡化版自動注入邏輯
//public void RegisterService<TInterface, TImplementation>()
//    where TImplementation : class, TInterface
//{
//    // 取得建構子
//    var constructor = typeof(TImplementation).GetConstructors()[0];
//    var parameterInfos = constructor.GetParameters();

//    // 自動尋找目前已註冊的參數
//    var parameters = parameterInfos.Select(p => _services[p.ParameterType]).ToArray();

//    // 實例化並註冊
//    var instance = Activator.CreateInstance(typeof(TImplementation), parameters) as TInterface;
//    _services[typeof(TInterface)] = instance;
//}