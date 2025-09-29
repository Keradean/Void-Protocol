using UnityEngine;

public class InteractTerminal : Interactable
{
    public override void Interaction()
    {

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return;

        // [AI-ASSISTED] Terminal activation audio feedback
        SoundManager.Instance?.PlayTerminalActivation(transform.position);

        // TODO: Add terminal functionality here

        base.Interaction();
    }
}
