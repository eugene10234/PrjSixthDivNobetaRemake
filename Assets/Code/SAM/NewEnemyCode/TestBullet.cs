using UnityEngine;
using Nobeta.Sam.Enemy;
using Assets.Code.EUG.AANEW.Temp;

public class TestBullet : MonoBehaviour
{
    public EnemyProvider ENEMY;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Debug.Log("Enemy Hit");
            ENEMY.TakeDamage(50, transform.position);
        }
    }




}
