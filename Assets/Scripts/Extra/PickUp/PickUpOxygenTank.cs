using UnityEngine;

public class PickUpOxygenTank : MonoBehaviour
{
    [SerializeField] private float oxygenAmount; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player == null) return;

            PlayerStats stats = player.Stats;

            if (stats.Oxy < stats.MaxOxy)
            {
                stats.Oxy += oxygenAmount;
                if (stats.Oxy > stats.MaxOxy) // Clamp 
                    stats.Oxy = stats.MaxOxy;// Clamp // Same as stats.Oxy = Mathf.Min(stats.Oxy + 30, stats.MaxOxy);

                Destroy(gameObject);
            }
        }
    }
}
