using Nobeta.EUG.FSM;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.FSM.Services;
using Nobeta.EUG.Settings;
using Nobeta.EUG.FSM.States;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PlayerLifetimeScope : LifetimeScope
{
    [Header("配置")]
    [SerializeField] private PlayerConfig playerConfig;

    [Header("組件")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    //[Header("除錯選項")]
    //[SerializeField] private bool showDebugInfo = true;
    //[SerializeField] private bool logStateTransitions = true;
    protected override void Awake()
    {
        // 驗證配置
        if (playerConfig == null)
        {
            Debug.LogError("PlayerConfig 未設置！");
        }

        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        base.Awake();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        Debug.Log("=== 配置 Player LifetimeScope ===");

        // 註冊配置（實例）
        if (playerConfig != null)
        {
            builder.RegisterInstance(playerConfig);
        }

        // 註冊組件（實例）
        //builder.RegisterInstance(characterController).WithParameter("debug", showDebugInfo);
        builder.RegisterInstance(characterController);
        builder.RegisterInstance(animator);
        builder.RegisterInstance(transform);

        // 註冊服務（Scoped - 每個 Player 一個實例）
        RegisterServices(builder);

        // 註冊核心（Scoped）
        builder.Register<StateContext>(Lifetime.Scoped);
        builder.Register<StateMachine>(Lifetime.Scoped);

        // 註冊所有狀態（Transient - 按需創建）
        RegisterStates(builder);

        builder.RegisterEntryPoint<PlayerControlPrensenter>();
        // 註冊 Component（自動從 Hierarchy 注入）
        builder.RegisterComponentInHierarchy<RootMotionProxy>();
        builder.RegisterComponentInHierarchy<RigHeadTargetController>();        
        //Debug.LogWarning("RegisterComponentInHierarchy<mbPlayerController>");
        //builder.RegisterComponentInHierarchy<mdler>();
        //var mbCharacterController = GetComponent<mbPlayerController>();
        //builder.RegisterComponent(mbCharacterController);
        //builder.RegisterComponent(playerController);

        Debug.Log("✓ Player LifetimeScope 配置完成");
    }

    private void RegisterServices(IContainerBuilder builder)
    {
        // 移動服務
        builder.Register<IMovementService, MovementService>(Lifetime.Scoped);
        //builder.Register<IPlayerInputService, PlayerInputService>(Lifetime.Singleton);


        //// 攝影機服務
        //builder.Register<ICameraService, CameraService>(Lifetime.Scoped);

        // 動畫服務
        builder.Register<IAnimationService, AnimationService>(Lifetime.Scoped);

        // 地面檢測服務
        builder.Register<IDetectionService, DetectionService>(Lifetime.Scoped);

        Debug.Log("✓ 服務註冊完成");
    }

    private void RegisterStates(IContainerBuilder builder)
    {
        // 地面狀態
        builder.Register<IdleState>(Lifetime.Transient);
        builder.Register<WalkingState>(Lifetime.Transient);
        builder.Register<RunningState>(Lifetime.Transient);
        builder.Register<TurningState>(Lifetime.Transient);

        // 空中狀態
        builder.Register<JumpingState>(Lifetime.Transient);
        builder.Register<FallingState>(Lifetime.Transient);
        builder.Register<LandingState>(Lifetime.Transient);

        // 防禦狀態
        builder.Register<DodgingState>(Lifetime.Transient);

        // 反應狀態
        builder.Register<HitState>(Lifetime.Transient);
        builder.Register<DyingState>(Lifetime.Transient);
        builder.Register<FellingState>(Lifetime.Transient);
        builder.Register<FellRecoveryState>(Lifetime.Transient);
        

        // 戰鬥狀態
        builder.Register<AimingState>(Lifetime.Transient);
        builder.Register<ChantingState>(Lifetime.Transient);
        builder.Register<MeleeAttackState>(Lifetime.Transient);

        // TODO: 添加更多狀態

        // builder.Register<ChargingState>(Lifetime.Transient);
        // builder.Register<CastingState>(Lifetime.Transient);

        Debug.Log("✓ 狀態註冊完成");
    }
}

