using UnityEngine;

public class PickUpHealth : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] public float HealthValue; // PickUpValue MediPen


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null && playerHealth.CanRestoreHealth())
            {
                PlayerStats stats = other.GetComponent<Player>().Stats;
                playerHealth.RestoreHealth(HealthValue);

                // AUDIO INTEGRATION - Added by Julian with AI-Support
                // Using pickup ammo sound as placeholder for health pickup
                SoundManager.Instance?.PlayPickupAmmo(transform.position);

                Destroy(gameObject);
            }

        }
    }
}