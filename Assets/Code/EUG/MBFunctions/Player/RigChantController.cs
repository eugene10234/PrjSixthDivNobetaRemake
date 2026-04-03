using DG.Tweening;
using Nobeta.EUG.BulletSystem.OOP.UnityBridge;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using VContainer;


public class RigChantController : MonoBehaviour
{
    //[Header("目標設定")]
    //public Transform target; // 想要看的目標

    [Header("Rig 設定")]
    public Rig rigChant;                   // 你的 Rig 元件
    //public Transform rigTarget;      // 你的 Animation Rigging Target 物件

    private Animator _playerAnimator;
    private readonly float HandChant_Duration = 0.5f;
    private bool isPlayerLive = true;
    private bool isChanting = false;
    private IEventBus _eventBus;

    private void Awake()
    {
        _eventBus = EventBusProvider.GetEventBus();
        _playerAnimator = GetComponent<Animator>();
        rigChant.weight = 0f;
        _playerAnimator.SetLayerWeight(3, 0f);
    }
    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }
    private void OnChantStart(RigChantStartEvent evt)
    {
        if (rigChant == null) return;
        if (!isPlayerLive) return;
        if (isChanting == true) return;
        isChanting = true;
        RigChantTweening(1f, Ease.OutQuad);
    }
    private void OnChantEnd(RigChantEndEvent evt)
    {
        if (rigChant == null) return;
        if (!isPlayerLive) return;
        if (isChanting == false) return;
        isChanting = false;
        RigChantTweening(0f, Ease.Linear);
    }
    private void RigChantTweening(float target, Ease easeType)
    {
        if (DOTween.IsTweening(rigChant)) DOTween.Kill(rigChant);
        DOTween.To(() => rigChant.weight, x => rigChant.weight = x, target, HandChant_Duration)
               .SetEase(easeType)
               .SetUpdate(UpdateType.Normal, false)
               .SetId(rigChant)
               .OnUpdate(() =>
               {
                   _playerAnimator.SetLayerWeight(3, rigChant.weight);
               });
    }


    private void OnChanfsfsftStart()
    {
        DOTween.To(() => rigChant.weight, x => rigChant.weight = x, 1f, HandChant_Duration)
                       .SetEase(Ease.Linear)
                       .SetUpdate(UpdateType.Normal, false)
                       .SetId(this);
    }
    private void OnPlayerLiveStateChange(PlayerLiveStateEvent evt)
    {
        isPlayerLive = evt.IsLive;
    }
    private void OnChantStart() { isChanting = true; /*OnProgress();*/ }
    private void OnChantEnd() { isChanting = false; /*OnProgress();*/ }
    private void SubscribeEvents()
    {
        _eventBus.Subscribe<PlayerLiveStateEvent>(OnPlayerLiveStateChange);
        _eventBus.Subscribe<RigChantStartEvent>(OnChantStart);
        _eventBus.Subscribe<RigChantEndEvent>(OnChantEnd);
    }
    private void UnsubscribeEvents()
    {
        _eventBus.Unsubscribe<PlayerLiveStateEvent>(OnPlayerLiveStateChange);
        _eventBus.Unsubscribe<RigChantStartEvent>(OnChantStart);
        _eventBus.Unsubscribe<RigChantEndEvent>(OnChantEnd);
    }
}
