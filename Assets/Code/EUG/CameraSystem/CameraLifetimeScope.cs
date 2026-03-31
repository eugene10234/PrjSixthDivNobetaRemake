using Nobeta.EUG.CameraSystem;
using Nobeta.EUG.FSM;
using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.FSM.Services;
using Nobeta.EUG.GameFlowTry.Menu;
using Nobeta.EUG.Services;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.Root
{
    public class CameraLifetimeScope : LifetimeScope
    {
        [SerializeField] private Camera _mainCamera;
        //protected override void Awake()
        //{
        //    // 驗證配置
        //    if (playerConfig == null)
        //    {
        //        Debug.LogError("PlayerConfig 未設置！");
        //    }

        //    if (characterController == null)
        //    {
        //        characterController = GetComponent<CharacterController>();
        //    }

        //    if (animator == null)
        //    {
        //        animator = GetComponent<Animator>();
        //    }

        //    base.Awake();
        //}
        protected override void Configure(IContainerBuilder builder)
        {
            //builder.Register<CameraService>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<CinemachineThirdPersonAim>();
            builder.RegisterEntryPoint<ThridPerCamPrensenter>(Lifetime.Scoped).AsSelf();
        }
    }
}
