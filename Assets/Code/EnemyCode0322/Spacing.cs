using Nobeta.Sam.Enemy;
using UnityEngine;

public class Spacing: MonoBehaviour
{
    private BigGhost bg;
    private bool isCalculating;
    private float radius = 20;
    int enemyMask;
    int obstacleMask;
    int combinedMask;
    private void Awake()
    {
        bg = GetComponent<BigGhost>();
        enemyMask = 1 << LayerMask.NameToLayer("Enemy");
        obstacleMask = 1 << LayerMask.NameToLayer("Obstacle");
        combinedMask = enemyMask | obstacleMask;
    }

    private void LateUpdate()
    {
        bg.InformSpacing(CalculateAvoidDirection());
    }
    public Vector3 CalculateAvoidDirection() 
    {
        if (isCalculating == true) return Vector3.zero;
        Collider[] collisions = Physics.OverlapSphere(transform.position, radius, combinedMask);
        if (collisions.Length == 0) return Vector3.zero;
        Vector3 avoidDirection = Vector3.zero;
        foreach (Collider collision in collisions)
        {
            if (collision.transform.root == transform) continue;

            Vector3 offset = transform.position - collision.transform.position;
            float dist = offset.magnitude;
            if (dist <= 0) continue;

            float weight = 1f - (dist / radius);

            // If it's a wall, give it more "push" power to prevent clipping
            if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                weight *= 3f;
            }
            //avoidDirection = normalized offset direction * weight
            avoidDirection += (offset / dist) * weight;
        }
        avoidDirection.y = 0f;
        return avoidDirection.normalized;
    }
}
