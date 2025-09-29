/*
====================================================================
PlayerHealth
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==================================================
// PLAYER HEALTH CLASS
// ==================================================
// This class manages the player's health system
// It handles taking damage, healing, and death
// Implements IDamageable interface so other scripts can damage the player
public class PlayerHealth : MonoBehaviour, IDamageable
{
    // ==================================================
    // VARIABLE DECLARATION
    // ==================================================

    [Header("Config")] // Creates a header in Unity Inspector called "Config"
    [SerializeField] private PlayerStats stats; // Reference to PlayerStats that stores current health and max health values

    [Header("References")] // Creates a header in Unity Inspector called "References"
    [SerializeField] private UIManager uiManager; // Reference to UIManager for showing death screen and health UI

    // ==================================================
    // AWAKE METHOD
    // ==================================================
    // Awake is called automatically by Unity before the game starts
    // Used for initialization and getting component references
    private void Awake()
    {
        // If UIManager reference is not assigned in the Inspector
        if (uiManager == null)
        {
            // Try to find UIManager component on the same GameObject
            uiManager = GetComponent<UIManager>();
        }
    }

    // ==================================================
    // TAKE DAMAGE METHOD
    // ==================================================
    // This public method is called when the player receives damage
    // Required by IDamageable interface - allows enemies/traps/hazards to damage the player
    public void TakeDamage(float amount)
    {
        // Safety check: if stats don't exist, exit method early (prevents errors)
        if (stats == null) return;

        // Subtract damage amount from player's current health
        stats.Health -= amount; // For example: if Health = 100 and amount = 25, Health becomes 75

        // Check if player's health dropped to zero or below
        if (stats.Health <= 0f)
        {
            // Set health to exactly 0 (prevents negative health values)
            stats.Health = 0f;

            // Call the death method
            PlayerDead();
        }
    }

    // ==================================================
    // RESTORE HEALTH METHOD
    // ==================================================
    // This public method is called to heal the player
    // Used by health pickups, medkits, regeneration systems, etc.
    public void RestoreHealth(float amount)
    {
        // Safety check: if stats don't exist, exit method early (prevents errors)
        if (stats == null) return;

        // Add healing amount to player's current health
        stats.Health += amount; // For example: if Health = 50 and amount = 30, Health becomes 80

        // Check if healing caused health to go above maximum
        if (stats.Health > stats.MaxHealth)
        {
            // Cap health at maximum (prevents overhealing)
            stats.Health = stats.MaxHealth; // For example: can't have 120/100 health
        }
    }

    // ==================================================
    // CAN RESTORE HEALTH METHOD
    // ==================================================
    // This public method checks if the player can be healed
    // Returns true if healing is possible, false if not
    // Useful for health pickups to check if they should heal the player
    public bool CanRestoreHealth()
    {
        // Safety check: if stats don't exist, return false
        if (stats == null) return false;

        // Return true only if:
        // 1. Player is alive (Health > 0) AND
        // 2. Player is not at full health (Health < MaxHealth)
        // This prevents healing dead players or players at full health
        return stats.Health > 0 && stats.Health < stats.MaxHealth;
    }

    // ==================================================
    // PLAYER DEAD METHOD
    // ==================================================
    // This private method is called when the player dies
    // Handles death screen display and any death-related logic
    private void PlayerDead()
    {
        // Safety check: if UIManager doesn't exist, exit method early (prevents errors)
        if (uiManager == null) return;

        // Show the death screen UI (Game Over screen, respawn options, etc.)
        uiManager.ShowDeathScreen();

        // Print death message to console (for debugging purposes)
        Debug.Log("I am Dead");
    }
}