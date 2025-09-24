using UnityEngine;

public class PickUpAmmo : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] WeaponsManager WeaponsManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (WeaponsManager != null)
            {
                WeaponsManager.AddAmmo(WeaponsManager.pickUpValue);
                Debug.Log("Ammo aufgesammelt!");
            }
            else
            {
                Debug.LogWarning("WeaponsController nicht zugewiesen!");
            }

            Destroy(gameObject);
        }
    }
}
