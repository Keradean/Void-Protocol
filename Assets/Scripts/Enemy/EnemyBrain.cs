using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    [SerializeField] private string initState; // Initial state ID to start the FSM // PatrolState, AttackState, etc.
    [SerializeField] private FSMState[] states; // Array of all possible states in the FSM 

    public FSMState CurrentState { get; set; }
    public Transform Player { get; set; }

    private void Start()
    {
        if (Player == null)
        {
            Player = GameObject.FindWithTag("Player")?.transform; // Sucht den Player per Tag
            if (Player == null)
            {
                Debug.LogError("Player nicht gefunden! Bitte den Player zuweisen.");
                return;
            }

            ChangeState(initState); // Initialize the FSM by setting the current state to the initial state
        }
    }

    private void Update()
    {
        if (CurrentState == null) return; 
        CurrentState.UpdateState(this);

        if (Player == null) return; 

        Vector3 directionToPlayer = new Vector3(Player.transform.position.x, transform.position.y, Player.transform.position.z) - transform.position;

        if (directionToPlayer.magnitude > 0.1f) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);  
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); 
        }
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
