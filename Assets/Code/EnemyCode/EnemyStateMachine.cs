using System.Collections;
using UnityEngine;

namespace Nobeta.Sam.Enemy
{
    public class EnemyStateMachine 
    {
        public enum EnemyState
        {
            patrol,
            attack,
            chase,
            flee,
            idle,
            death,
        }

    }
}