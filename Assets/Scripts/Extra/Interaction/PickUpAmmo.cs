using UnityEngine;

public class PickUpAmmo : Interactable
{
   
    public override void Interaction()
    {
        //Debug.Log("Heb es auf Junge!");

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return;

        WeaponsManager weaponsManager = playerObject.GetComponentInChildren<WeaponsManager>();
        if (weaponsManager == null) return;

        weaponsManager.AddAmmo(weaponsManager.pickUpValue);


        // Julian [AI-ASSISTED] Pickup audio feedback - consistent with PickUpHealth pattern
        SoundManager.Instance?.PlayPickupItem(transform.position);

        base.Interaction();
        Destroy(gameObject);
    }
}

