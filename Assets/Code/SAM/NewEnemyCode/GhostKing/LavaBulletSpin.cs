using UnityEngine;

public class LavaBulletSpin : MonoBehaviour
{
    public float spinSpeed;

    public void OnEnable()
    {
        spinSpeed = 300f;
    }
    void FixedUpdate()
    {
        // Create a rotation of 'X' degrees around the X-axis (Vector3.right)
        Quaternion xStep = Quaternion.Euler(spinSpeed * Time.deltaTime, 0, 0);

        // Apply it to the current rotation
        transform.localRotation *= xStep *= xStep;
    }
}
