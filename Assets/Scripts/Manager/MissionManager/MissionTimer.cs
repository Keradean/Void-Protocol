/*
====================================================================
* MissionTimer
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
// MISSION TIMER CLASS
// ==================================================
// This is a ScriptableObject that represents a time-based mission
// Inherits from Mission base class and adds countdown timer functionality
//
// SCRIPTABLE OBJECT:
// ScriptableObjects are data containers that exist as assets (not in scenes)
// They're perfect for missions, weapons, items, etc. because you can:
// - Create multiple instances with different values (Mission1, Mission2, etc.)
// - Edit values in the Project window without needing a scene
// - Share data between scenes
//
// [CreateAssetMenu] allows you to create new instances:
// Right-click in Project → Create → Missions → MissonsTimer (note: typo in original)
//
// MISSION TYPE:
// This is a countdown timer mission - player must survive/complete objectives before time runs out
// Example: "Survive for 5 minutes" or "Reach the extraction point in 3 minutes"
[CreateAssetMenu(fileName = "New Timer for Missions", menuName = "Missions/MissonsTimer")]
public class MissionTimer : Mission
{
    // ==================================================
    // VARIABLE DECLARATION
    // ==================================================

    // Total time for the mission in seconds
    // Example: 300f = 5 minutes (300 seconds)
    // Set in Unity Inspector when creating the ScriptableObject asset
    public float time;

    // Current remaining time (counts down from 'time' to 0)
    // When this reaches 0, mission fails
    private float currentTime;

    // ==================================================
    // START MISSION METHOD (OVERRIDE)
    // ==================================================
    // This method overrides the base Mission class's StartMission()
    // Called when the mission begins (initializes the timer)
    public override void StartMission()
    {
        // Set current time to the full mission time (start countdown)
        // Example: if time = 300, currentTime starts at 300 and counts down
        currentTime = time;
    }

    // ==================================================
    // UPDATE MISSION METHOD (OVERRIDE)
    // ==================================================
    // This method overrides the base Mission class's UpdateMission()
    // Called every frame while the mission is active (decreases timer)
    public override void UpdateMission()
    {
        // Decrease timer by the time that passed since last frame
        // Time.deltaTime is the frame duration (e.g. 0.016s for 60 FPS)
        // This makes the countdown frame-rate independent (same speed on all computers)
        currentTime -= Time.deltaTime;

        // Check if time has run out
        if (currentTime < 0)
        {
            // TODO: Implement game over / mission failed logic
            // Currently commented out - would trigger failure state
            //  Debug.Log("Game Over Looser!!!");
        }

        // Convert remaining seconds to MM:SS format (minutes:seconds)
        // System.TimeSpan.FromSeconds converts float seconds to TimeSpan
        // ToString("mm\\:ss") formats as 05:30, 01:45, etc.
        // Example: 325.7 seconds → "05:25" (5 minutes, 25 seconds)
        string timeText = System.TimeSpan.FromSeconds(currentTime).ToString("mm\\:ss");

        // TODO: Display timeText in UI
        // Currently commented out - would show timer to player
        // Debug.Log(timeText);
    }

    // ==================================================
    // MISSION COMPLETED METHOD (OVERRIDE)
    // ==================================================
    // This method overrides the base Mission class's MissionCompleted()
    // Returns whether the mission was completed successfully
    //
    // RETURN VALUE:
    // true = mission succeeded (time remaining > 0)
    // false = mission failed (time ran out)
    //
    // LOGIC:
    // For a timer mission, success = finishing objectives BEFORE time runs out
    // If currentTime > 0 when checking, player still has time (mission can succeed)
    // If currentTime ≤ 0, time ran out (mission failed)
    public override bool MissionCompleted()
    {
        // Return true if there's still time remaining
        // Example: currentTime = 45.2 → returns true (still have time)
        // Example: currentTime = -2.1 → returns false (time ran out)
        return currentTime > 0;
    }
}