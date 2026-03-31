using Nobeta.EUG.GameFlowTry.Core.Save;
using Nobeta.EUG.GameFlowTry.Core.Scene;
using Nobeta.EUG.GameFlowTry.Game;
using Nobeta.EUG.GameFlowTry.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.EUG.GameFlowTry.DI
{
    using VContainer.Unity;

    namespace Nobeta.EUG.GameFlowTry.DI
    {
        /// <summary>
        /// 唯一職責：在容器初始化時強制解析 IGameManager，
        /// 確保 GameManager 建構子執行、EventBus 訂閱生效。
        /// </summary>
        public class AppInitializer : IStartable
        {
            private 
            public AppInitializer(IGameManager gameManager)
            {
                // 注入 IGameManager 即可，GameManager 建構子此時必定執行
            }

            public void Start() { }
        }
    }
}
