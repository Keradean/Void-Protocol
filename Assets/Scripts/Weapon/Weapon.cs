/*
====================================================================
Weapon
====================================================================
Project: Space Colony Game
Course: PIP
Script-Developer: Dennis De Col 
*
WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
Diese detaillierte Authorship-Dokumentation ist für die
akademische Bewertung erforderlich und darf nicht entfernt werden!
*
====================================================================
*/
using UnityEngine;

// ==================================================
// WEAPON CLASS
// ==================================================
// This class stores all data/properties for a weapon in the game
// It's a data container that holds weapon statistics and references
// This is typically used by WeaponsManager to handle different weapon types
//
// NOTE: This class only STORES weapon data, it doesn't handle shooting logic
// The actual shooting, reloading, and weapon switching is handled by WeaponsManager
//
// USAGE:
// Attach this script to weapon GameObjects (pistol, rifle, shotgun, etc.)
// Each weapon prefab will have different values for range, damage, ammo, etc.
public class Weapon : MonoBehaviour
{
    // ==================================================
    // WEAPON STATISTICS - RANGE & EFFECTS
    // ==================================================

    // Maximum distance the weapon can shoot (in Unity units)
    // Example: 100 = can hit targets up to 100 units away
    // Used for raycast distance or projectile max range
    public float Range;

    // Reference to the muzzle flash effect GameObject (visual effect when shooting)
    // This is typically a particle system or sprite that appears at the gun barrel
    public GameObject MuzzleFlare;

    // How long the muzzle flash is visible (in seconds)
    // Example: 0.1f = flash appears for 0.1 seconds then disappears
    public float FlareDisplayTime;

    // ==================================================
    // WEAPON BEHAVIOR - FIRE MODE
    // ==================================================

    // Determines if the weapon fires automatically when holding the trigger
    // true = automatic fire (machine gun, submachine gun)
    // false = semi-automatic fire (pistol, shotgun - one shot per trigger press)
    public bool AutoFire;

    // Delay between shots (fire rate) in seconds
    // Example: 0.1f = 10 shots per second, 0.5f = 2 shots per second
    // Smaller value = faster fire rate, larger value = slower fire rate
    public float TimeBtwShots;

    // ==================================================
    // AMMUNITION SYSTEM
    // ==================================================

    // Current ammunition in the weapon's magazine/clip (ready to fire)
    // Example: 15 = 15 bullets currently loaded in the gun
    // When this reaches 0, the weapon needs to reload
    public int CurrentAmmo;

    // Maximum ammunition capacity of the magazine/clip
    // Example: 30 = can hold up to 30 bullets in one magazine
    // Used to determine how much ammo is restored when reloading
    public int ClipSize;

    // Total ammunition available for reloading (reserve ammo)
    // Example: 120 = 120 bullets in reserve (can reload multiple times)
    // This decreases when reloading, increases when picking up ammo
    public int RemainingAmmo;

    // How much ammo is added when picking up an ammo pack for this weapon
    // Example: 30 = picking up ammo gives 30 extra bullets to RemainingAmmo
    public int pickUpValue;

    // ==================================================
    // DAMAGE SYSTEM
    // ==================================================

    // Amount of damage this weapon deals per hit
    // Example: 25.0f = deals 25 damage to enemies (if enemy has 100 health, 4 shots to kill)
    // Higher value = more powerful weapon
    public float damage;
}