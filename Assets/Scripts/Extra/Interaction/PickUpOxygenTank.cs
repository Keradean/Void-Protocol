using UnityEngine;

public class PickUpOxygenTank : Interactable
{
    [SerializeField] private float oxygenValue;

    public override void Interaction()
    {
        //Debug.Log("Interaction Baby!!!");

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return;

        Player player = playerObject.GetComponent<Player>();
        if (player == null) return;

        PlayerStats stats = player.Stats;
        if (stats == null) return;

        if (stats.Oxy < stats.MaxOxy)
        {
            stats.Oxy += oxygenValue;
            stats.Oxy = Mathf.Min(stats.Oxy, stats.MaxOxy);

            // Julian [AI-ASSISTED] Pickup audio feedback - consistent pattern
            SoundManager.Instance?.PlayPickupItem(transform.position);

            base.Interaction();
            Destroy(gameObject);
        }
    }
}
