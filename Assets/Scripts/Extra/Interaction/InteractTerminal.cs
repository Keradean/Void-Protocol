/*
====================================================================
* InteractTerminal
====================================================================
* Project: Void Protocol
* Course: PIP
* Script-Developer: Dennis De Col
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
====================================================================
*/
using UnityEngine;

// ==================================================
// INTERACT TERMINAL CLASS
// ==================================================
// This class represents a terminal/computer that the player can interact with
// Inherits from Interactable base class, adding mission start and spawner activation functionality
//
// TYPICAL USE CASE:
// Mission briefing terminal that:
// 1. Starts a mission when activated
// 2. Activates enemy spawners (begins the challenge)
// 3. Can only be used once (prevents re-activation)
//
// EXAMPLE SCENARIO:
// Player approaches terminal → sees interaction prompt → presses E →
// Mission countdown starts + enemies begin spawning
public class InteractTerminal : Interactable
{
    // ==================================================
    // VARIABLE DECLARATION - MISSION SETTINGS
    // ==================================================

    [Header("Mission Start Settings")] // Header in Unity Inspector
    // Should this terminal start a mission when activated?
    // true = starts mission automatically, false = only activates spawner
    [SerializeField] private bool startMissionOnInteract = true;

    // ==================================================
    // VARIABLE DECLARATION - SPAWNER SETTINGS
    // ==================================================

    [Header("Spawner Settings")] // Header in Unity Inspector
    // Reference to the enemy spawner that should be activated
    // Assign in Unity Inspector - drag spawner GameObject into this field
    [SerializeField] private Spawner enemySpawner;

    // ==================================================
    // VARIABLE DECLARATION - STATE TRACKING
    // ==================================================

    // Has this terminal been activated before?
    // Prevents player from re-activating the terminal multiple times
    private bool hasBeenActivated = false;

    // ==================================================
    // INTERACTION METHOD (OVERRIDE)
    // ==================================================
    // This method overrides the base Interactable.Interaction()
    // Called when player presses interact button while near this terminal
    // Starts mission and activates enemy spawner (if configured)
    public override void Interaction()
    {
        // ==================================================
        // ACTIVATION CHECK
        // ==================================================
        // If terminal was already activated, do nothing (exit early)
        // Prevents mission from restarting or spawner from being re-enabled
        if (hasBeenActivated) return;

        // ==================================================
        // PLAYER VALIDATION
        // ==================================================
        // Find the player GameObject in the scene by tag
        // Used to verify interaction is coming from player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        // If player doesn't exist, exit early (safety check)
        if (playerObject == null) return;

        // ==================================================
        // AUDIO FEEDBACK
        // ==================================================
        // [AI-ASSISTED] Terminal activation audio feedback
        // Play sound effect at terminal's position (beep, activation sound, etc.)
        // ?. is null-conditional operator (only calls if SoundManager.Instance exists)
        SoundManager.Instance?.PlayTerminalActivation(transform.position);

        // ==================================================
        // MISSION START
        // ==================================================
        // If mission start is enabled AND MissionManager exists
        if (startMissionOnInteract && MissionManager.instance != null)
        {
            // Start the current mission (timer countdown, objective tracking, etc.)
            // ?. is null-conditional operator (only calls if currentMission exists)
            MissionManager.instance.currentMission?.StartMission();

            // Print confirmation to console (for debugging)
            Debug.Log("Mission gestartet!");
        }

        // ==================================================
        // SPAWNER ACTIVATION
        // ==================================================
        // If enemy spawner reference exists
        if (enemySpawner != null)
        {
            // Enable the spawner component (starts spawning enemies)
            // Before this, spawner is disabled and does nothing
            enemySpawner.enabled = true;

            // Print confirmation to console (for debugging)
            Debug.Log("Enemy Spawner aktiviert!");
        }
        else // Spawner reference is missing (not assigned in Inspector)
        {
            // Print warning to console (helps developers catch setup errors)
            Debug.LogWarning("Kein Enemy Spawner zugewiesen!");
        }

        // ==================================================
        // STATE UPDATE
        // ==================================================
        // Mark terminal as activated (prevents re-use)
        hasBeenActivated = true;

        // Call the base class Interaction method
        // This allows base Interactable to run any additional logic (if needed)
        base.Interaction();
    }
}