using Nobeta.Sam.Enemy;
using System.Linq;
using UnityEngine;

public class AggroSensor : MonoBehaviour
{
    private BigGhost bg;
    private int playerLayerMask; 
    private void Awake()
    {
        bg = GetComponent<BigGhost>();
        playerLayerMask = 1 << LayerMask.NameToLayer("Player");
    }

    private void FixedUpdate()
    {
        CheckAggro();
    }
    private float _aggroTimer = 2;
    private bool isStandBy = true;
    private void CheckAggro()
    {
        Collider[] collisions = Physics.OverlapSphere(transform.position, 25f, playerLayerMask);
        if (collisions.Length > 0)
        { 
            bg.InformTarget(collisions[0].gameObject);
            _aggroTimer = 2;
            isStandBy = false;
        }
        if (collisions.Length == 0 && !isStandBy)
        {
            _aggroTimer-= Time.fixedDeltaTime;
        }
        if (_aggroTimer <= 0)
        {
            bg.InformTargetLost();
            _aggroTimer = 2;
            isStandBy = true;
        }
    }

    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, 25f);
    //}
}
