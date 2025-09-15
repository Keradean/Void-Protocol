using System;

[Serializable]
public class FSMTransition
{
    public FSMDecision Decision; // PlayerInRangeofAttack -> true or false
    public string TrueState; // CurrentAction -> AttackAction
    public string FalseState; // CurrentAction -> PatrolAction
}
