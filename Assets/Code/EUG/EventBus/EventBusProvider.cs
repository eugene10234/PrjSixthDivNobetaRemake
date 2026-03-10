// ========================================================================
// EventBusProvider - EventBus 提供者（單例）
// ========================================================================

using Nobeta.EUG.Interfaces;
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