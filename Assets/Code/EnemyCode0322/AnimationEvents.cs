using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public HitBox hitbox;
    private void Awake()
    {
    }

    public void AE_ActivateHitArea()
    {
        if (hitbox != null)
        {
            hitbox.ActivateHitArea();
        }
        else
        {
            Debug.Log("hit box is null");
        }
    }

    public void DeActivateHitArea()
    {
        if (hitbox != null)
        {
            hitbox.DeActivateHitArea();
        }
        else
        {
            Debug.Log("hit box is null");
        }
    }
}
