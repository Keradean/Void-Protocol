using UnityEngine;

public class WeaponsController : MonoBehaviour
{
    [Header("Confiq")]
    [SerializeField] private float range; 
    [SerializeField] private Transform mainCam; 
    [SerializeField] private LayerMask validLayers;
    [SerializeField] private GameObject impactEffect, damageEffect;
    [SerializeField] private GameObject muzzelFlare;
    [SerializeField] private float flareTime;
    [SerializeField] private float flareCount;
    [SerializeField] private int currentAmmo;
    [SerializeField] private int clipSize;
    [SerializeField] private int remainingAmmo;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       if(flareCount > 0)
        {
          flareCount -= Time.deltaTime;
            if(flareCount <= 0)
            {
                muzzelFlare.SetActive(false);
            }
        }
    }

    public void Shoot() 
    {
        if (currentAmmo > 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(mainCam.position, mainCam.forward, out hit, range, validLayers))
            {
                Debug.Log(hit.transform.name);

                if (hit.transform.tag == "Enemy")
                {
                    Instantiate(damageEffect, hit.point, Quaternion.identity);
                }
                else
                {

                    Instantiate(impactEffect, hit.point, Quaternion.identity);

                }
            }
            muzzelFlare.SetActive(true);
            flareCount = flareTime;
            currentAmmo--;
        }
    }

    public void Reload()
    {
        // Reload Logic 
        remainingAmmo += currentAmmo;

        if (remainingAmmo >= clipSize) 
        { 
        Debug.Log("I am Reloading my Weapon");
        currentAmmo = clipSize;
        remainingAmmo -= clipSize;
        }
        else
        {
            currentAmmo = remainingAmmo;
            remainingAmmo = 0;
        }
        //PlayreloadAnimation(); // if its a littel bit Time lef´t
    }
}
