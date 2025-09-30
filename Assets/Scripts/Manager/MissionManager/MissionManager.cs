/*
====================================================================
* MissionManager
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
// MISSION MANAGER CLASS
// ==================================================
// This class manages the active mission in the game
// It uses a Singleton pattern (static instance) for easy global access
// Handles starting missions, updating them each frame, and checking completion status
//
// SINGLETON PATTERN:
// Unlike inheriting from Singleton<T>, this uses manual singleton implementation
// Access from anywhere with: MissionManager.instance
//
// RESPONSIBILITIES:
// - Store reference to current active mission
// - Start missions (initialize mission state)
// - Update missions every frame (countdown timers, track objectives, etc.)
// - Check if mission is completed
public class MissionManager : MonoBehaviour
{
    // ==================================================
    // VARIABLE DECLARATION - SINGLETON
    // ==================================================

    // Static instance for singleton pattern (only one MissionManager exists)
    // "static" means this belongs to the class itself, not individual objects
    // Other scripts can access this with: MissionManager.instance
    public static MissionManager instance;

    // ==================================================
    // VARIABLE DECLARATION - MISSION STATE
    // ==================================================

    // Reference to the currently active mission (ScriptableObject)
    // This could be a MissionTimer, MissionKill, MissionCollect, etc.
    // Assigned in Unity Inspector or through code
    public Mission currentMission;

    // Boolean flag tracking whether a mission has been started
    // false = mission not started yet, true = mission is active
    private bool missionStarted = false;

    // ==================================================
    // AWAKE METHOD
    // ==================================================
    // Awake is called when the script instance is being loaded (before Start)
    // Sets up the singleton instance
    private void Awake()
    {
        // Set this instance as THE singleton instance
        // NOTE: This doesn't check for duplicates - if multiple MissionManagers exist,
        // the last one created will become the instance (overwrites previous)
        // A more robust implementation would destroy duplicates or use DontDestroyOnLoad
        instance = this;
    }

    // ==================================================
    // UPDATE METHOD
    // ==================================================
    // Update is called every frame by Unity
    // Updates the current mission if one is active
    private void Update()
    {
        // Only update mission if one has been started
        if (missionStarted)
        {
            // Call the mission's UpdateMission method
            // The ?. is null-conditional operator (only calls if currentMission isn't null)
            // This prevents errors if currentMission is somehow null
            // Example: Timer missions countdown, objective missions check progress
            currentMission?.UpdateMission();
        }
    }

    // ==================================================
    // START MISSION METHOD
    // ==================================================
    // This public method starts the current mission
    // Called by other scripts (e.g. when player enters mission area, clicks "Start", etc.)
    // Initializes the mission and sets it to active state
    public void StartMission()
    {
        // Check if there's a mission assigned AND it hasn't been started yet
        if (currentMission != null && !missionStarted)
        {
            // Call the mission's StartMission method (initializes mission state)
            // Example: Timer missions set currentTime = time, objective missions reset counters
            currentMission.StartMission();

            // Mark mission as started (allows Update to run mission logic)
            missionStarted = true;

            // Print mission start message to console (for debugging)
            // $"..." is string interpolation - inserts missionName variable into text
            // Example: "Mission 'Survive 5 Minutes' gestartet!"
            Debug.Log($"Mission '{currentMission.missionName}' gestartet!");
        }
    }

    // ==================================================
    // MISSION COMPLETED METHOD
    // ==================================================
    // This public method checks if the current mission is completed
    // Called by other scripts to determine if mission objectives are met
    //
    // RETURN VALUE:
    // true = mission completed successfully
    // false = mission not completed OR no mission exists
    //
    // EXPRESSION-BODIED MEMBER:
    // => is a shorthand for { return ...; }
    // This entire method body is just one return statement
    //
    // NULL-COALESCING OPERATOR (??):
    // currentMission?.MissionCompleted() ?? false means:
    // - If currentMission exists: call MissionCompleted() and return its result
    // - If currentMission is null: return false
    // This prevents null reference errors
    public bool MissionCompleted() => currentMission?.MissionCompleted() ?? false;
}