using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Interfaces;
using System;
using UnityEngine;
using VContainer;
namespace Nobeta.EUG.FSM.Services
{
    public class RootMotionProxy : MonoBehaviour
    {
        // 定義一個 Action，讓外部 Service 可以訂閱
        public System.Action<Vector3, Quaternion> OnMoveDelta;

        private Animator _animator;

        void Awake() => _animator = GetComponent<Animator>();

        void OnAnimatorMove()
        {
            // 將數據轉發出去，而不是在這裡處理邏輯
            OnMoveDelta?.Invoke(_animator.deltaPosition, _animator.deltaRotation);
        }
    }
}
