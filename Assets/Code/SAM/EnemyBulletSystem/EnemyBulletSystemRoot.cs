using UnityEngine;

namespace Nobeta.Sam.Bullet
{
    /// <summary>
    /// Empty MonoBehaviour placed on a GameObject in the EnemyManager scene.
    /// Gives BulletSystem a stable Transform to parent pooled bullets under,
    /// keeping the hierarchy clean.
    ///
    /// RegisterComponent this in EnemyManager.Configure().
    /// </summary>
    public class EnemyBulletSystemRoot : MonoBehaviour { }
}
