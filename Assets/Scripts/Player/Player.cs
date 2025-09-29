/*
====================================================================
Player
====================================================================
Project: Space Colony Game
Course: PIP
Script-Developer: Dennis De Col 
*
WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
Diese detaillierte Authorship-Dokumentation ist für die
akademische Bewertung erforderlich und darf nicht entfernt werden!
*
Dieses Script wurde in einer voherigen Abgabe verwendet. 
The Last Refuge / 3D Interactive
====================================================================
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==================================================
// PLAYER CLASS
// ==================================================
// This is the main Player class that manages the player character
// It holds references to other important player components like health and stats
public class Player : MonoBehaviour
{
    // ==================================================
    // VARIABLE DECLARATION
    // ==================================================

    [Header("Config")] // Creates a header in Unity Inspector called "Config"
    [SerializeField] private PlayerStats stats; // Reference to the PlayerStats scriptable object (stores player data like health, speed, etc.)

    // Public property to access PlayerStats from other scripts
    // The => syntax means this property just returns the stats variable
    public PlayerStats Stats => stats; // Other scripts can read player stats through this

    // Property to access PlayerHealth component
    // { get; private set; } means: other scripts can read it, but only this script can change it
    public PlayerHealth PlayerHealth { get; private set; } // Manages the player's health system

    //private PlayerAnimations animations; // Reference to PlayerAnimations for handling animations (currently not used/commented out)

    // ==================================================
    // AWAKE METHOD
    // ==================================================
    // Awake is called automatically by Unity when the game object is created
    // This happens before Start() and before the game begins
    private void Awake()
    {
        // Get the PlayerHealth component that is attached to this same GameObject
        // GetComponent searches for a component of type PlayerHealth on this object
        PlayerHealth = GetComponent<PlayerHealth>(); // Initialize the PlayerHealth reference
    }

    // ==================================================
    // UPDATE METHOD
    // ==================================================
    // Update is called every frame by Unity
    // Currently this method is empty (no code inside)
    private void Update()
    {
        // Empty for now - can be used later for per-frame updates
    }

    // ==================================================
    // RESET STATS METHOD
    // ==================================================
    // This public method can be called from other scripts to reset the player's stats
    // For example: resetting health to full, resetting speed to default, etc.
    public void ResetStats()
    {
        stats.ResetStats(); // Call the ResetStats method from the PlayerStats scriptable object
    }
}