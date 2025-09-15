using System; // This namespace is used to organize the FSM related classes

// This class represents a state in a finite state machine (FSM) for an enemy AI system.
// It can be extended to include specific behaviors and properties for each state.

[Serializable] // This attribute allows the class to be serialized, which is useful for Unity's inspector and serialization system.
public class FSMState // AttackState
{
    public string ID; // Unique identifier for the state (e.g., "PatrolState", "AttackState", etc.)
    public FSMAction[] Actions; // MoveAction, AttackAction, PatrolAction, etc. | Array of actions that can be performed in this state
    public FSMTransition[] Transitions; // Array of transitions to other states based on decisions

    public void UpdateState(EnemyBrain enemyBrain)
    {
        ExecuteActions(); // Execute all actions associated with this state
        ExecuteTransitions(enemyBrain);
    }

    private void ExecuteActions()
    {
        for (int i = 0; i < Actions.Length; i++)
        {
            Actions[i].Act(); // Execute each action in the Actions array
        }
    }

    private void ExecuteTransitions(EnemyBrain enemyBrain)
    {
        if (Transitions == null || Transitions.Length <= 0) return; // If there are no transitions, exit the method
        for (int i = 0; i < Transitions.Length; i++)
        {
            bool value = Transitions[i].Decision.Decide(); // Corrected: Access the 'decision' field directly and call Decide()
            if (value)
            {
                enemyBrain.ChangeState(Transitions[i].TrueState); // Change state based on the true action of the transition 
            }
            else
            {
                enemyBrain.ChangeState(Transitions[i].FalseState); // Change state based on the false action of the transition
            }
        }
    }
}