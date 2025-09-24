using UnityEngine;

public class WeaponsManager : MonoBehaviour
{
    public float Range;
    public Transform Cam;
    public LayerMask ValidLayers;
    public GameObject ImpactEffect, DamageEffect;
    public GameObject MuzzleFlare;
    public float FlareDisplayTime;
    private float FlareCounter;

    public bool AutoFire;
    public float TimeBtwShots;
    private float ShotCounter;

    public int CurrentAmmo;
    public int ClipSize;
    public int RemainingAmmo;

    public int pickUpValue;
    public float damage;

    public Weapon[] Weapons;

    public UIManager UIManager;

    private int CurrentWeapon; // PreviousWeapon; 



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (UIManager == null)
            UIManager = FindFirstObjectByType<UIManager>();

        SetWeapon(0); 
    }

    // Update is called once per frame
    void Update()
    {
        if (FlareCounter > 0) 
        {
            FlareCounter -= Time.deltaTime;
            if (FlareCounter <= 0 && MuzzleFlare != null)
            {
                MuzzleFlare.SetActive(false);
            }
        }
        // ShotCounter immer runterzählen, damit Feuerrate auch bei Einzelfeuer funktioniert
        if (ShotCounter > 0)
            ShotCounter -= Time.deltaTime;

        UpdateAmmoUI();
    }

    public void Shoot()
    {
        // Einzelfeuer und Feuerrate korrekt behandeln
        if (CurrentAmmo > 0 && ShotCounter <= 0f)
        {
            RaycastHit hit;
            if (Physics.Raycast(Cam.position, Cam.forward, out hit, Range))
            {
                Debug.Log(hit.transform.name);
                if (hit.transform.CompareTag("Enemy"))
                {
                    IDamageable damageable = hit.transform.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(damage);
                        Instantiate(DamageEffect, hit.point, Quaternion.identity);
                    }
                    else
                    {
                        Instantiate(ImpactEffect, hit.point, Quaternion.identity);
                    }
                }

                if (MuzzleFlare != null)
                {
                    MuzzleFlare.SetActive(true);
                }
                FlareCounter = FlareDisplayTime;

                CurrentAmmo--;
                UpdateAmmoUI();
            }
            ShotCounter = TimeBtwShots; 
        }
    }
  


    public void ShootHeld()
    {
      
        if (AutoFire == true)
        {
            ShotCounter -= Time.deltaTime;
            if (ShotCounter <= 0f)
            {
                Shoot();
            }
        }
    }

    public void Reload()
    {
        if (CurrentAmmo >= ClipSize || RemainingAmmo <= 0) return;
        Debug.Log("Lad nach!!");
        RemainingAmmo += CurrentAmmo;
        if (RemainingAmmo >= ClipSize)
        {
            CurrentAmmo = ClipSize;
            RemainingAmmo -= ClipSize;
        }
        else
        {
            CurrentAmmo = RemainingAmmo;
            RemainingAmmo = 0;
        }
    }

    public void AddAmmo(int pickUpValue)
    {
        RemainingAmmo += pickUpValue;
        Debug.Log($"Munition aufgefüllt! + {pickUpValue}.Gesamt: {RemainingAmmo}");
    }

    // Methode zum Waffenwechsel
    public void SetWeapon(int weaponToSet)
    {
        Weapons[CurrentWeapon].CurrentAmmo = CurrentAmmo;
        Weapons[CurrentWeapon].RemainingAmmo = RemainingAmmo;



        Range = Weapons[weaponToSet].Range;
        FlareDisplayTime = Weapons[weaponToSet].FlareDisplayTime;
        AutoFire = Weapons[weaponToSet].AutoFire;
        TimeBtwShots = Weapons[weaponToSet].TimeBtwShots;
        CurrentAmmo = Weapons[weaponToSet].CurrentAmmo;
        ClipSize = Weapons[weaponToSet].ClipSize;
        RemainingAmmo = Weapons[weaponToSet].RemainingAmmo;
        pickUpValue = Weapons[weaponToSet].pickUpValue;
        damage = Weapons[weaponToSet].damage;
        MuzzleFlare = Weapons[weaponToSet].MuzzleFlare;

        foreach (Weapon w in Weapons)
        {
            w.gameObject.SetActive(false);
        }

        Weapons[weaponToSet].gameObject.SetActive(true);

        UpdateAmmoUI();
    }

    public void UpdateAmmoUI()
    {
        if (UIManager != null)
        {
            UIManager.ammoTMP.text = $"{CurrentAmmo} / {RemainingAmmo}";

            if (ClipSize > 0)
                UIManager.ammoBar.fillAmount = (float)CurrentAmmo / (float)ClipSize;
            else
                UIManager.ammoBar.fillAmount = 0;
        }
    }

    public void NextWeapon()
    {
        CurrentWeapon++;
        if(CurrentWeapon >= Weapons.Length)
        {
            CurrentWeapon = 0; 
        }

        SetWeapon(CurrentWeapon);
    }

    public void PreviousWeapon()
    {
        CurrentWeapon--;
        if (CurrentWeapon < 0) 
        {
            CurrentWeapon = Weapons.Length - 1;
        }

        SetWeapon(CurrentWeapon);
    }
}
