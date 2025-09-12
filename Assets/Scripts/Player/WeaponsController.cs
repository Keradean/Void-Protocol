using UnityEngine;

public class WeaponsController : MonoBehaviour
{
    [Header("Config")]
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

    [Header("Stats Reference")]
    [SerializeField] private PlayerStats playerStats; // Reference to PlayerStats

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (flareCount > 0)
        {
          flareCount -= Time.deltaTime;
            if (flareCount <= 0)
            {
                muzzelFlare.SetActive(false);
            }
        }
    }

    public void Shoot() 
    {
        if (playerStats.CurrentAmmo > 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(mainCam.position, mainCam.forward, out hit, range, validLayers))
            {
                Debug.Log(hit.transform.name);
                if (hit.transform.CompareTag("Enemy"))
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
            playerStats.CurrentAmmo--; // Use PlayerStats
        }
    }

    public void Reload()
    {
        // Reload Logic using PlayerStats
        playerStats.RemainingAmmo += playerStats.CurrentAmmo;

        if (playerStats.RemainingAmmo >= playerStats.ClipSize) 
        { 
        Debug.Log("I am Reloading my Weapon");
        playerStats.CurrentAmmo = playerStats.ClipSize;
        playerStats.RemainingAmmo -= playerStats.ClipSize;
        }
        else
        {
            playerStats.CurrentAmmo = playerStats.RemainingAmmo;
            playerStats.RemainingAmmo = 0;
        }
        //PlayreloadAnimation(); // if its a littel bit Time lef´t
    }
}
