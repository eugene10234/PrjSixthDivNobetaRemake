using Cysharp.Threading.Tasks;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Contract for the animation actor of any enemy type.
    /// States call these — they never touch the Animator directly.
    /// </summary>
    public interface IAnimationActor
    {
        void PlayHit();
        UniTask PlayAttack();
        void PlayVictory();
        void PlayDeath();

        /// <summary>
        /// Async dissolve-out sequence. Awaited by the death state before Destroy().
        /// </summary>
        UniTask PlayDeathSequence(float duration);
    }
}
