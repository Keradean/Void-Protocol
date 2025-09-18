using UnityEngine;

public class PickUpAmmo : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            FindFirstObjectByType<WeaponsController>().GetAmmo();

            Destroy(gameObject);

            Debug.Log("Aufgesammelt");
        }
    }
}
