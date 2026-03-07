using Nobeta.EUG.Events;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.FSM.States;
using System;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.EUG.CameraSystem
{
    public class ThridPerCamPrensenter : IInitializable, ITickable,ILateTickable, IDisposable
    {
        //private readonly StateContext _context;
        private readonly IPlayerInputService _inputService;
        private ICameraService _cameraService;
        private CinemachineThirdPersonAim _cinemachineThirdPersonAim;
        private CinemachineCamera _cinemachineCamera;

        //private readonly IEventBus _eventBus;
        //private readonly DebugServiceMb _debugService;

        public ThridPerCamPrensenter(CinemachineThirdPersonAim cinemachineThirdPersonAim, IPlayerInputService inputService, ICameraService cameraService)
        {
            _cinemachineThirdPersonAim = cinemachineThirdPersonAim;
            _inputService = inputService;
            _cameraService = cameraService;
        }
        public void Initialize()
        {
            _cinemachineCamera = _cinemachineThirdPersonAim.GetComponentInChildren<CinemachineCamera>();
            _cameraService.Initial(_cinemachineCamera);
        }
        public void Dispose()
        {

        }
        public void Tick()
        {

        }
        public void LateTick()
        {
            _cameraService.ExecuteCameraRotation();
        }

    }
}
