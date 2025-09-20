using UnityEngine;

public class PickUpAmmo : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int pickUpValue;
    [SerializeField] private PlayerStats playerStats;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            GetAmmo();

            Destroy(gameObject);

            Debug.Log("Aufgesammelt");
        }
    }

    public void GetAmmo()
    {
        playerStats.RemainingAmmo += pickUpValue;
        Debug.Log("Nimm mich du Sau");
    }
}
