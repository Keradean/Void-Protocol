using UnityEngine;

public class PickUpHealth : Interactable
{
    [Header("Config")]
    [SerializeField] private float healthValue;

    public override void Interaction()
    {
        base.Interaction();
        Debug.Log("Bring es mir Junge!");


        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return;


        Player player = playerObject.GetComponent<Player>();
        if (player == null) return;

        PlayerStats stats = player.Stats;
        PlayerHealth playerHealth = player.PlayerHealth;
        if (stats == null || playerHealth == null) return;

        if (playerHealth.CanRestoreHealth())
        {
            playerHealth.RestoreHealth(healthValue);

            Destroy(gameObject);
        }
    }
    public override string GetInteractionText()
    {
        return "Press E to Heal";
    }

}
