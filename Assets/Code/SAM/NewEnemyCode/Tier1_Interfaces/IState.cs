namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Contract for every state in every enemy's state machine.
    /// States are plain C# classes — no MonoBehaviour, no Unity lifecycle.
    /// </summary>
    public interface IState
    {
        /// <summary>Called once when the state machine transitions INTO this state.</summary>
        void OnEnter();

        /// <summary>Called every FixedUpdate tick while this state is active.</summary>
        void OnFixedTick();

        /// <summary>Called once when the state machine transitions OUT of this state.</summary>
        void OnExit();
    }
}
