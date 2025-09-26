using UnityEngine;

public class InteractTerminal : Interactable
{
    public override void Interaction()
    {

        base.Interaction();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return;
    }
}
