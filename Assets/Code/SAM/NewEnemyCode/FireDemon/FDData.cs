using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{

    public class FDData : BaseEnemyData
    {
        [Inject]
        public FDData(EnemyStat stat) : base(stat)
        {
            Debug.Log("FireDemon Data Ready");
        }
    }
}
