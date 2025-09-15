using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    [SerializeField] private string initState; // Initial state ID to start the FSM // PatrolState, AttackState, etc.
    [SerializeField] private FSMState[] states; // Array of all possible states in the FSM 

    public FSMState CurrentState { get; set; }
    public Transform Player { get; set; }

    private void Start()
    {
        ChangeState(initState); // Initialize the FSM by setting the current state to the initial state
    }

    private void Update()
    {
        if (CurrentState == null) return; // Ensure the current state is valid before updating
        CurrentState.UpdateState(this); // Update the current state of the enemy(this === enemyBrain)
    }

    public void ChangeState(string newStateID)
    {
        FSMState newState = GetState(newStateID); // Get the new state based on the provided ID
        if (newState == null) return; // Ensure the new state is valid before changing
        CurrentState = newState; // Update the current state to the new state   
    }

    private FSMState GetState(string newStateID)
    {
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].ID == newStateID)
            {
                return states[i]; // Return the state with the matching ID
            }
        }
        return null; // Ensure all code paths return a value | Copilot suggested this, but it was necessary in this context
    }
}
