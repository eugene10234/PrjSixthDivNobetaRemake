using UnityEngine;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Contract for the movement actor of any enemy type.
    /// Shared states (ChaseState, ReturnHomeState) depend only on this — never on a concrete actor.
    /// Concrete implementations are MonoBehaviours registered via RegisterComponent().As&lt;IMovementActor&gt;().
    /// 
    /// RULE: Never expose MonoBehaviour concerns here (transform, gameObject, GetComponent, Awake, etc.).
    /// If a state needs world position, it reads Position — not transform.position directly.
    /// </summary>
    public interface IMovementActor
    {
        Vector3 Position { get; }

        /// <summary>Move toward world-space target using this enemy's speed and pathfinding.</summary>
        void MoveToward(Vector3 target);

        /// <summary>Apply lateral avoidance push from the spacing system.</summary>
        void ApplyAvoidance(Vector3 direction);

        /// <summary>Navigate back to spawn position.</summary>
        void ReturnHome();

        /// <summary>Zero velocity, stay in place.</summary>
        void Stop();

        /// <summary>Face toward target instantly or via slerp — actor decides.</summary>
        void LookAt(Vector3 target);

        void ClearPath();

        public bool CheckReturnHome();

        void DeathMovement();
    }
}
