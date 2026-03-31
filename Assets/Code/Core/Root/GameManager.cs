using System;
using UnityEngine;
using VContainer.Unity;

namespace Assets.Scripts.RooteXPIRE
{
    public class GameManager : IInitializable, IDisposable
    {
        public void Initialize()
        {
            Debug.Log("GameManager Initialize");
        }
        public void Print()
        {
            Debug.Log("GameManager Initialize");
        }

        public void Dispose()
        {
            Debug.Log("GameManager Dispose");
        }
    }
}
