using UnityEngine;

public class PickUpHealth : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField]public float HealthValue; // PickUpValue MediPen


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null && playerHealth.CanRestoreHealth())
            {
                PlayerStats stats = other.GetComponent<Player>().Stats;
                playerHealth.RestoreHealth(HealthValue);

                Destroy(gameObject);
            }

        }
    }
}