using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{

    public class FDData : BaseEnemyData
    {
        public string Name = "憤怒的烈焰生骸";
        public string Reward = "烈火魔法";
        
        [Inject]
        public FDData(EnemyStat stat) : base(stat)
        {
            Debug.Log("FireDemon Data Ready");
        }
    }
}
