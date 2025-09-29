/*
====================================================================
* WeaponsManager.cs - Combat Audio Integration v3.1
====================================================================
* Project: Void Protocol
* Course: PIP
* Script-Developer: Dennis De Col
* Created: 2025-08-25
* Last Modified: 2025-09-28
* Version: v3.1 - Audio Integration Applied
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
* AUDIO INTEGRATION ATTRIBUTION:
* [HUMAN-AUTHORED] - Combat Audio Integration Konzept von Julian Gomez
* [AI-ASSISTED] - SoundManager Integration Implementierung
* 
* BEREINIGUNGSNOTIZEN v3.1:
* - Combat Audio Integration durch Julian Gomez hinzugefügt
* - Weapon/Impact/Reload Audio Calls implementiert
* - Empty Weapon Audio Feedback hinzugefügt
====================================================================
*/

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

    private int CurrentWeapon, previouWeapons;

    void Awake()
    {
        if (UIManager == null)
            UIManager = FindFirstObjectByType<UIManager>();

        SetWeapon(0);
    }

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

        if (ShotCounter > 0)
            ShotCounter -= Time.deltaTime;

        UpdateAmmoUI();
    }

    public void Shoot()
    {
        if (CurrentAmmo > 0 && ShotCounter <= 0f)
        {
            // AUDIO INTEGRATION - Added by Julian with AI-Support
            SoundManager.Instance?.PlayWeaponSound(CurrentWeapon, transform.position);

            RaycastHit hit;
            if (Physics.SphereCast(Cam.position, 0.5f, Cam.forward, out hit, Range))
            {
                if (hit.transform.CompareTag("Enemy"))
                {
                    IDamageable damageable = hit.transform.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(damage);

                        // AUDIO INTEGRATION - Added by Julian with AI-Support
                        SoundManager.Instance?.PlayImpactSoundMobs(hit.point);

                        Instantiate(DamageEffect, hit.point, Quaternion.identity);
                    }
                }
                else
                {
                    // AUDIO INTEGRATION - Added by Julian with AI-Support
                    SoundManager.Instance?.PlayImpactSoundObjects(hit.point);

                    Instantiate(ImpactEffect, hit.point, Quaternion.identity);
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
        else if (CurrentAmmo <= 0)
        {
            // AUDIO INTEGRATION - Added by Julian with AI-Support
            SoundManager.Instance?.PlayWeaponEmpty(transform.position);
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

        // AUDIO INTEGRATION - Added by Julian with AI-Support
        SoundManager.Instance?.PlayReloadSound(transform.position);

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

        UpdateAmmoUI();
    }

    public void AddAmmo(int pickUpValue)
    {
        RemainingAmmo += pickUpValue;
    }

    public void SetWeapon(int weaponToSet)
    {
        if (previouWeapons != CurrentWeapon)
        {
            Weapons[previouWeapons].CurrentAmmo = CurrentAmmo;
            Weapons[previouWeapons].RemainingAmmo = RemainingAmmo;
        }

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

        previouWeapons = CurrentWeapon;
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
        if (CurrentWeapon >= Weapons.Length)
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