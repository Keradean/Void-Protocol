using UnityEngine;

public class PickUpHealth : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null && playerHealth.CanRestoreHealth())
            {
                PlayerStats stats = other.GetComponent<Player>().Stats;
                playerHealth.RestoreHealth(stats.HealthValue);

                Destroy(gameObject);
            }

        }
    }
}