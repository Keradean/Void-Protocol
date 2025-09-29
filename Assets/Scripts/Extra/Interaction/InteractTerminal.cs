using UnityEngine;

public class InteractTerminal : Interactable
{
    public override void Interaction()
    {

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return;

        base.Interaction();
    }
}
