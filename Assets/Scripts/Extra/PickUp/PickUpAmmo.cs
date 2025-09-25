using UnityEngine;

public class PickUpAmmo : Interactable
{
   
    public override void Interaction()
    {
        base.Interaction();

        Debug.Log("Heb es auf Junge!");

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return;

        WeaponsManager weaponsManager = playerObject.GetComponentInChildren<WeaponsManager>();
        if (weaponsManager == null) return;

        weaponsManager.AddAmmo(weaponsManager.pickUpValue);

        Destroy(gameObject);
    }

    public override string GetInteractionText()
    {
        return "Press E to Pick Up Ammo";
    }

}

