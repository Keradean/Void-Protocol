using UnityEngine;

public class PlayerOxygen : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Oxygen Settings")]
    [SerializeField] private float oxygenDrainRate; 
    [SerializeField] private float damageWhenNoOxygen; 

    private void Update()
    {
        if (stats == null || playerHealth == null) return;

        DrainOxygen();
    }

    private void DrainOxygen()
    {
        if (stats.Oxy > 0f)
        {
            stats.Oxy -= oxygenDrainRate * Time.deltaTime;
        }
        else
        {
            stats.Oxy = 0f;
            playerHealth.TakeDamage(damageWhenNoOxygen * Time.deltaTime);
        }
    }

    public void RefillOxygen(float amount)
    {
        stats.Oxy += amount;
        if (stats.Oxy > stats.MaxOxy)
        {
            stats.Oxy = stats.MaxOxy;
        }
    }
}
