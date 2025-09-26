using UnityEngine;

public class MissionsEnd : MonoBehaviour
{
    private GameObject player;

    private void Start()
    {
        player = GameObject.Find("Player"); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != player) return;
        Debug.Log("Du hast es geschafft du 1 gluk");
    }
}
