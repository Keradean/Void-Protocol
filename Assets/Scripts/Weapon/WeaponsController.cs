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


    [Header("Fire Settings")]
    [SerializeField] private bool canAutoFire;
    [SerializeField] private float autoFireRate;
    [SerializeField] private float singleShotCooldown;
    private float shootCounter;

    [Header("Stats Reference")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private float damageValue;

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

        if (shootCounter > 0)
        {
            shootCounter -= Time.deltaTime;
        }
    }

    public void Shoot()
    {
        if (shootCounter > 0) return;

        if (playerStats.CurrentAmmo > 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(mainCam.position, mainCam.forward, out hit, range, validLayers))
            {
                Debug.Log(hit.transform.name);

                if (hit.transform.CompareTag("Enemy"))
                {
                    IDamageable enemy = hit.transform.GetComponent<IDamageable>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damageValue);
                    }
                    Instantiate(damageEffect, hit.point, Quaternion.identity);
                }
                else
                {
                    Instantiate(impactEffect, hit.point, Quaternion.identity);
                }
            }

            muzzelFlare.SetActive(true);
            flareCount = flareTime;
            playerStats.CurrentAmmo--;


            if (canAutoFire)
            {
                shootCounter = autoFireRate;
            }
            else
            {
                shootCounter = singleShotCooldown;
            }
        }
    }

    public void Reload()
    {
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
        //PlayreloadAnimation(); // if its a little bit Time left
    }
}