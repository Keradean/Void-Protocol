using UnityEngine;

public class WeaponsController : MonoBehaviour
{
    [Header("Confiq")]
    [SerializeField] private float range; 
    [SerializeField] private Transform mainCam; 
    [SerializeField] private LayerMask validLayers; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Shoot() 
    {
        RaycastHit hit;
        if (Physics.Raycast(mainCam.position, mainCam.forward, out hit, range, validLayers))
        {
            Debug.Log(hit.transform.name);
        }
        
        //Debug.Log("Sterbi!!");
    }
}
