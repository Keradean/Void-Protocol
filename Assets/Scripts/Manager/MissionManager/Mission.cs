/*
====================================================================
* Mission
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
// MISSION CLASS (ABSTRACT BASE CLASS)
// ==================================================
// This is an abstract base class for all mission types in the game
// It's a ScriptableObject, meaning it exists as an asset file (not in scenes)
//
// ABSTRACT CLASS:
// An abstract class is like a template/blueprint that cannot be used directly
// You MUST create child classes that inherit from it (MissionTimer, MissionKill, etc.)
// Abstract classes can have abstract methods (must be implemented by children)
// and virtual methods (can be optionally overridden by children)
//
// WHY SCRIPTABLE OBJECT?
// ScriptableObjects are perfect for mission data because:
// - You can create multiple mission assets with different settings
// - Mission data persists between scenes
// - Easy to edit in Unity Inspector without coding
// - Memory efficient (shared data, not duplicated per instance)
//
// CHILD CLASSES MUST IMPLEMENT:
// - StartMission() - what happens when mission begins
// - MissionCompleted() - how to check if mission is done
//
// EXAMPLE CHILD CLASSES:
// - MissionTimer : Mission (survive for X seconds)
// - MissionKill : Mission (eliminate X enemies)
// - MissionCollect : Mission (gather X items)
public abstract class Mission : ScriptableObject
{
    // ==================================================
    // VARIABLE DECLARATION - MISSION DATA
    // ==================================================

    // Name of the mission displayed to the player
    // Example: "Survive the Onslaught", "Reach the Extraction Point", "Eliminate All Hostiles"
    public string missionName;

    // Detailed description of mission objectives
    // [TextArea] attribute makes this a multi-line text field in Unity Inspector
    // This makes it easier to write longer descriptions without scrolling horizontally
    // Example: "You have been deployed to a hostile sector. Survive for 5 minutes
    // until the extraction team arrives. Watch your oxygen levels and ammunition."
    [TextArea] public string missiondescription;

    // ==================================================
    // START MISSION METHOD (ABSTRACT)
    // ==================================================
    // This abstract method MUST be implemented by all child classes
    // Called when the mission begins (initializes mission state)
    //
    // "abstract" means:
    // - No implementation here (no method body)
    // - Child classes MUST provide their own implementation
    // - Each mission type has different starting logic
    //
    // EXAMPLES OF IMPLEMENTATIONS:
    // - MissionTimer: set currentTime = maxTime
    // - MissionKill: set enemiesKilled = 0
    // - MissionCollect: set itemsCollected = 0
    public abstract void StartMission();

    // ==================================================
    // MISSION COMPLETED METHOD (ABSTRACT)
    // ==================================================
    // This abstract method MUST be implemented by all child classes
    // Returns whether the mission objectives have been completed
    //
    // "abstract" means:
    // - No implementation here (no method body)
    // - Child classes MUST provide their own implementation
    // - Each mission type has different completion criteria
    //
    // RETURN VALUE:
    // true = mission completed successfully
    // false = mission not yet completed or failed
    //
    // EXAMPLES OF IMPLEMENTATIONS:
    // - MissionTimer: return currentTime > 0 (time remaining)
    // - MissionKill: return enemiesKilled >= targetKills
    // - MissionCollect: return itemsCollected >= targetAmount
    public abstract bool MissionCompleted();

    // ==================================================
    // UPDATE MISSION METHOD (VIRTUAL)
    // ==================================================
    // This virtual method CAN be optionally overridden by child classes
    // Called every frame while the mission is active (via MissionManager.Update)
    //
    // "virtual" means:
    // - Has a default implementation (empty in this case)
    // - Child classes CAN override it if needed, but don't have to
    // - Useful for missions that need per-frame updates
    //
    // WHEN TO OVERRIDE:
    // Override this if your mission needs continuous updates:
    // - MissionTimer: countdown timer every frame
    // - MissionSurvival: check player health every frame
    // - MissionEscape: calculate distance to exit every frame
    //
    // WHEN NOT TO OVERRIDE:
    // Don't override if mission is event-based:
    // - MissionKill: only updates when enemy dies (not every frame)
    // - MissionCollect: only updates when item collected (not every frame)
    //
    // Base implementation is empty - does nothing by default
    public virtual void UpdateMission()
    {
        // Empty by default - child classes override to add per-frame logic
    }
}