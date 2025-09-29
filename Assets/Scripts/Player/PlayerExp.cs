/*
====================================================================
PlayerExp
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==================================================
// PLAYER EXP CLASS
// ==================================================
// This class manages the player's experience (EXP) system
// It handles gaining experience points and leveling up when enough EXP is collected
public class PlayerExp : MonoBehaviour
{
    // ==================================================
    // VARIABLE DECLARATION
    // ==================================================

    [Header("Config")] // Creates a header in Unity Inspector called "Config"
    [SerializeField] private PlayerStats stats; // Reference to PlayerStats script that stores experience and level data

    // ==================================================
    // UPDATE METHOD
    // ==================================================
    // Update is called every frame by Unity
    // Currently used for testing the experience system
    private void Update()
    {
        // For testing purposes only - press L key to gain experience
        if (Input.GetKeyDown(KeyCode.L)) // Check if the L key was pressed this frame
        {
            AddExp(300f); // Add 300 experience points to test the system
        }
    }

    // ==================================================
    // ADD EXP METHOD
    // ==================================================
    // This public method adds experience points to the player
    // Other scripts can call this when player defeats enemies, completes quests, etc.
    public void AddExp(float amount)
    {
        // Add the experience amount to the player's current experience total
        stats.CurrentExp += amount; // Increase current EXP (e.g. if player had 100 EXP and gains 300, now has 400 EXP)

        // Check if player has enough experience to level up (can level up multiple times at once)
        // While loop continues as long as current EXP is greater than or equal to EXP needed for next level
        while (stats.CurrentExp >= stats.NextLevelExp) // For example: if CurrentExp = 500 and NextLevelExp = 200
        {
            // Subtract the experience needed for leveling up from current experience
            stats.CurrentExp -= stats.NextLevelExp; // Leftover EXP carries over to next level (500 - 200 = 300 remaining)

            // Call the level up function
            NextLevel(); // Increase player level and calculate new EXP requirement
        }
    }

    // ==================================================
    // NEXT LEVEL METHOD
    // ==================================================
    // This private method handles the level up process
    // It increases the player's level and calculates the new experience requirement
    private void NextLevel()
    {
        // Increase player's level by 1
        stats.Level++; // For example: Level 1 becomes Level 2

        // Store the current experience requirement (before we change it)
        float currentExpRequired = stats.NextLevelExp; // For example: 200 EXP was needed

        // Calculate the new experience requirement for the NEXT level
        // Formula: Current requirement + (Current requirement × Multiplier ÷ 100)
        // Example: If NextLevelExp = 200 and ExpMultiplier = 50, then: 200 + (200 × 50/100) = 200 + 100 = 300
        // Mathf.Round rounds the result to the nearest whole number
        float newNextLevelExp = Mathf.Round(currentExpRequired + stats.NextLevelExp * (stats.ExpMultiplier / 100f));

        // Update the experience requirement for the next level
        stats.NextLevelExp = newNextLevelExp; // Now player needs more EXP to reach the next level (makes leveling progressively harder)
    }
}