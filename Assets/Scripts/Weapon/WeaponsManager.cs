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
* WICHTIG: KOMMENTIERUNG NICHT L�SCHEN!
* Diese detaillierte Authorship-Dokumentation ist f�r die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
* AUDIO INTEGRATION ATTRIBUTION:
* [HUMAN-AUTHORED] - Combat Audio Integration Konzept von Julian Gomez
* [AI-ASSISTED] - SoundManager Integration Implementierung
* 
* BEREINIGUNGSNOTIZEN v3.1:
* - Combat Audio Integration durch Julian Gomez hinzugef�gt
* - Weapon/Impact/Reload Audio Calls implementiert
* - Empty Weapon Audio Feedback hinzugef�gt
====================================================================
*/

/*
====================================================================
WeaponsManager
====================================================================
Project: Space Colony Game
Course: PIP
Script-Developer: Dennis De Col 
*
WICHTIG: KOMMENTIERUNG NICHT L�SCHEN!
Diese detaillierte Authorship-Dokumentation ist f�r die
akademische Bewertung erforderlich und darf nicht entfernt werden!
*
====================================================================
*/
using UnityEngine;

// ==================================================
// WEAPONS MANAGER CLASS
// ==================================================
// This class manages all weapon-related functionality in the game
// It handles: shooting, reloading, weapon switching, ammo management, and visual effects
// This is the central system that connects weapon data (from Weapon.cs) with gameplay logic
//
// RESPONSIBILITIES:
// - Shooting mechanics (raycasting to hit enemies)
// - Automatic vs semi-automatic fire
// - Ammunition tracking and reloading
// - Weapon switching (cycling through multiple weapons)
// - Visual effects (muzzle flash, impact effects)
// - UI updates (ammo counter, ammo bar)
public class WeaponsManager : MonoBehaviour
{
    // ==================================================
    // VARIABLE DECLARATION - WEAPON STATISTICS
    // ==================================================

    // Maximum shooting range of the current weapon (in Unity units)
    public float Range;

    // Reference to the camera transform (used for shooting direction)
    // Weapons shoot in the direction the camera is looking
    public Transform Cam;

    // LayerMask that determines which objects can be hit by bullets
    // Example: only hit enemies and environment, not UI or player
    public LayerMask ValidLayers;

    // Visual effect prefabs for bullet impacts
    public GameObject ImpactEffect; // Effect for hitting non-damageable objects (walls, ground, etc.)
    public GameObject DamageEffect; // Effect for hitting damageable objects (enemies, destructibles)

    // Reference to the active muzzle flash effect GameObject
    public GameObject MuzzleFlare;

    // How long the muzzle flash stays visible (in seconds)
    public float FlareDisplayTime;

    // Timer that counts down the muzzle flash display time
    private float FlareCounter;

    // ==================================================
    // VARIABLE DECLARATION - FIRE RATE CONTROL
    // ==================================================

    // Is this weapon automatic (holds trigger = continuous fire)?
    // true = automatic (machine gun), false = semi-automatic (pistol)
    public bool AutoFire;

    // Delay between shots (fire rate) in seconds
    // Example: 0.1f = 10 shots per second
    public float TimeBtwShots;

    // Timer that counts down between shots (prevents shooting too fast)
    private float ShotCounter;

    // ==================================================
    // VARIABLE DECLARATION - AMMUNITION SYSTEM
    // ==================================================

    // Current ammunition in the magazine/clip (ready to shoot)
    public int CurrentAmmo;

    // Maximum capacity of the magazine/clip
    public int ClipSize;

    // Total reserve ammunition available for reloading
    public int RemainingAmmo;

    // Amount of ammo gained when picking up ammo packs
    public int pickUpValue;

    // Damage dealt per bullet hit
    public float damage;

    // ==================================================
    // VARIABLE DECLARATION - WEAPON SWITCHING
    // ==================================================

    // Array of all available weapons the player can use
    public Weapon[] Weapons;

    // Reference to UIManager for updating ammo displays
    public UIManager UIManager;

    // Index of currently equipped weapon in the Weapons array
    // Index of previously equipped weapon (for saving ammo state)
    private int CurrentWeapon, previouWeapons;

    // ==================================================
    // AWAKE METHOD
    // ==================================================
    // Awake is called when the script instance is being loaded (before Start)
    // Used for initialization before the game starts
    void Awake()
    {
        // If UIManager reference is not assigned, find it in the scene
        if (UIManager == null)
            UIManager = FindFirstObjectByType<UIManager>();

        // Equip the first weapon (index 0) at game start
        SetWeapon(0);
    }

    // ==================================================
    // UPDATE METHOD
    // ==================================================
    // Update is called every frame by Unity
    // Handles timers and continuous updates
    void Update()
    {
        // ==================================================
        // MUZZLE FLASH TIMER
        // ==================================================
        // Count down the muzzle flash display timer
        if (FlareCounter > 0)
        {
            FlareCounter -= Time.deltaTime; // Decrease timer by frame time

            // When timer reaches 0, hide the muzzle flash
            if (FlareCounter <= 0 && MuzzleFlare != null)
            {
                MuzzleFlare.SetActive(false); // Disable the muzzle flash GameObject
            }
        }

        // ==================================================
        // FIRE RATE TIMER
        // ==================================================
        // Count down the shot cooldown timer (time between shots)
        if (ShotCounter > 0)
            ShotCounter -= Time.deltaTime; // Decrease timer by frame time

        // Update the ammo display in the UI every frame
        UpdateAmmoUI();
    }

    // ==================================================
    // SHOOT METHOD
    // ==================================================
    // This public method handles a single shot
    // Called when player presses fire button (for semi-automatic weapons)
    // Also called by ShootHeld() for automatic weapons
    public void Shoot()
    {
        // Check if weapon can shoot: has ammo AND fire rate cooldown is finished
        if (CurrentAmmo > 0 && ShotCounter <= 0f)
        {
            // Variable to store raycast hit information
            // AUDIO INTEGRATION - Added by Julian with AI-Support
            //SoundManager.Instance?.PlayWeaponSound(CurrentWeapon, transform.position);

            // Variable to store raycast hit information
            RaycastHit hit;

            // Perform a SphereCast (thick raycast) from camera forward
            if (Physics.SphereCast(Cam.position, 0.5f, Cam.forward, out hit, Range, ValidLayers))
            {
                // Print the name of the object that was hit (for debugging)
                Debug.Log("Hit: " + hit.transform.name);

                // Check if the hit object is tagged as "Enemy"
                if (hit.transform.CompareTag("Enemy"))
                {
                    // Try to get IDamageable component from the hit object
                    IDamageable damageable = hit.transform.GetComponent<IDamageable>();

                    // If the object can take damage
                    if (damageable != null)
                    {
                        // Deal damage to the enemy
                        damageable.TakeDamage(damage);
                        // Spawn damage effect (blood, sparks, etc.) at hit point
                        // AUDIO INTEGRATION - Added by Julian with AI-Support
                        //SoundManager.Instance?.PlayImpactSoundMobs(hit.point);
                        Instantiate(DamageEffect, hit.point, Quaternion.identity);
                    }
                    else // Enemy doesn't have IDamageable (shouldn't happen, but safety check)
                    {
                        // AUDIO INTEGRATION - Added by Julian with AI-Support
                        //SoundManager.Instance?.PlayImpactSoundObjects(hit.point);
                        // Spawn generic impact effect
                        Instantiate(ImpactEffect, hit.point, Quaternion.identity);
                    }
                }
                else // Hit something that's not an enemy (wall, ground, etc.)
                {
                    // AUDIO INTEGRATION - Added by Julian with AI-Support
                    //SoundManager.Instance?.PlayImpactSoundObjects(hit.point);
                    // Spawn generic impact effect
                    Instantiate(ImpactEffect, hit.point, Quaternion.identity);
                }
            }

            // WICHTIG: Dieser Code muss NACH dem Raycast-Check kommen,
            // aber AUSSERHALB des if(Physics.SphereCast) Blocks!
            // Er wird ausgeführt JEDES MAL wenn geschossen wird (egal ob getroffen oder nicht)

            // Show muzzle flash effect (if it exists)
            if (MuzzleFlare != null)
            {
                MuzzleFlare.SetActive(true);
            }

            // Start the muzzle flash timer
            FlareCounter = FlareDisplayTime;

            // Decrease ammunition by 1
            CurrentAmmo--;

            // Update the UI to show new ammo count
            UpdateAmmoUI();

            // Start the fire rate cooldown timer (prevents shooting again immediately)
            ShotCounter = TimeBtwShots;
        }
        else if (CurrentAmmo <= 0)
        {
            // AUDIO INTEGRATION - Added by Julian with AI-Support
           // SoundManager.Instance?.PlayWeaponEmpty(transform.position);
        }
    }


    // ==================================================
    // SHOOT HELD METHOD
    // ==================================================
    // This public method handles automatic fire when holding the trigger
    // Called every frame when fire button is held down
    public void ShootHeld()
    {
        // Only proceed if this weapon is automatic
        if (AutoFire == true)
        {
            // Count down the shot timer
            ShotCounter -= Time.deltaTime;

            // When timer reaches 0, shoot and reset timer
            if (ShotCounter <= 0f)
            {
                Shoot(); // Fire a shot (which will reset ShotCounter)
            }
        }
    }

    // ==================================================
    // RELOAD METHOD
    // ==================================================
    // This public method handles weapon reloading
    // Called when player presses reload button
    public void Reload()
    {
        // Don't reload if: already full OR no reserve ammo
        if (CurrentAmmo >= ClipSize || RemainingAmmo <= 0) return;

        // Print reload message to console (for debugging)
        Debug.Log("Lad nach!!");

        // Return current ammo to reserve pool (simulate removing partially-full magazine)
        // AUDIO INTEGRATION - Added by Julian with AI-Support
        SoundManager.Instance?.PlayReloadSound(transform.position);
        RemainingAmmo += CurrentAmmo;

        // Check if we have enough reserve ammo to fill a full clip
        if (RemainingAmmo >= ClipSize)
        {
            // Fill the clip completely
            CurrentAmmo = ClipSize;
            // Subtract clip size from reserve
            RemainingAmmo -= ClipSize;
        }
        else // Not enough ammo for a full clip
        {
            // Load all remaining ammo into the clip
            CurrentAmmo = RemainingAmmo;
            // Reserve is now empty
            RemainingAmmo = 0;
        }

        UpdateAmmoUI();
    }

    // ==================================================
    // ADD AMMO METHOD
    // ==================================================
    // This public method adds ammunition to reserve
    // Called when player picks up ammo packs
    //
    // PARAMETERS:
    // int pickUpValue - Amount of ammo to add to reserve
    public void AddAmmo(int pickUpValue)
    {
        // Add ammo to reserve pool
        RemainingAmmo += pickUpValue;
    }

    // ==================================================
    // SET WEAPON METHOD
    // ==================================================
    // This public method switches to a specific weapon
    // Saves current weapon's ammo state and loads new weapon's data
    //
    // PARAMETERS:
    // int weaponToSet - Index of weapon in Weapons array to equip
    public void SetWeapon(int weaponToSet)
    {
        // Save current weapon's ammo state before switching
        // Only if we're actually switching weapons (not initial setup)
        if (previouWeapons != CurrentWeapon)
        {
            // Save current ammo to the previous weapon's data
            Weapons[previouWeapons].CurrentAmmo = CurrentAmmo;
            Weapons[previouWeapons].RemainingAmmo = RemainingAmmo;
        }

        // ==================================================
        // LOAD NEW WEAPON'S DATA
        // ==================================================
        // Copy all stats from the new weapon to active variables
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

        // ==================================================
        // UPDATE VISUAL WEAPON MODELS
        // ==================================================
        // Hide all weapon GameObjects first
        foreach (Weapon w in Weapons)
        {
            w.gameObject.SetActive(false); // Disable each weapon's 3D model
        }

        // Show only the newly equipped weapon's GameObject
        Weapons[weaponToSet].gameObject.SetActive(true);

        // Update UI to show new weapon's ammo
        UpdateAmmoUI();

        // Remember this weapon as the previous weapon for next switch
        previouWeapons = CurrentWeapon;
    }

    // ==================================================
    // UPDATE AMMO UI METHOD
    // ==================================================
    // This public method updates the ammo display in the UI
    // Shows current ammo / reserve ammo and updates the ammo bar fill
    public void UpdateAmmoUI()
    {
        // Only update if UIManager exists
        if (UIManager != null)
        {
            // Update ammo text (e.g. "15 / 120")
            UIManager.ammoTMP.text = $"{CurrentAmmo} / {RemainingAmmo}";

            // Update ammo bar fill amount (0.0 to 1.0)
            if (ClipSize > 0) // Prevent division by zero
                // Cast to float for proper division (15/30 = 0.5 = 50% filled)
                UIManager.ammoBar.fillAmount = (float)CurrentAmmo / (float)ClipSize;
            else
                UIManager.ammoBar.fillAmount = 0; // Empty bar if no clip size
        }
    }

    // ==================================================
    // NEXT WEAPON METHOD
    // ==================================================
    // This public method switches to the next weapon in the array
    // Called when player presses "next weapon" button (e.g. mouse wheel up)
    public void NextWeapon()
    {
        // Increment weapon index
        CurrentWeapon++;

        // If we've gone past the last weapon, loop back to first weapon
        if (CurrentWeapon >= Weapons.Length)
        {
            CurrentWeapon = 0; // Reset to index 0 (first weapon)
        }

        // Equip the new weapon
        SetWeapon(CurrentWeapon);
    }

    // ==================================================
    // PREVIOUS WEAPON METHOD
    // ==================================================
    // This public method switches to the previous weapon in the array
    // Called when player presses "previous weapon" button (e.g. mouse wheel down)
    public void PreviousWeapon()
    {
        // Decrement weapon index
        CurrentWeapon--;

        // If we've gone below the first weapon, loop to last weapon
        if (CurrentWeapon < 0)
        {
            CurrentWeapon = Weapons.Length - 1; // Set to last index in array
        }

        // Equip the new weapon
        SetWeapon(CurrentWeapon);
    }
}