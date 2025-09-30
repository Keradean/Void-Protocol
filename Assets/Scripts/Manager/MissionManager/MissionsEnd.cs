/*
====================================================================
* MissionsEnd
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
// MISSIONS END CLASS
// ==================================================
// This class detects when the player reaches the mission end point/extraction zone
// It's typically attached to a trigger collider at the mission objective location
// When player enters the trigger, the mission is completed
//
// USAGE:
// 1. Create a GameObject at the mission end location (extraction point, exit, goal)
// 2. Add a Collider component and check "Is Trigger"
// 3. Attach this script to the GameObject
// 4. Player entering the trigger will complete the mission
//
// EXAMPLE SCENARIOS:
// - Extraction zone where player escapes
// - Finish line in a race mission
// - Safe zone to reach in a survival mission
public class MissionsEnd : MonoBehaviour
{
    // ==================================================
    // VARIABLE DECLARATION
    // ==================================================

    // Reference to the player GameObject
    // Used to verify that the player (not another object) entered the trigger
    private GameObject player;

    // ==================================================
    // START METHOD
    // ==================================================
    // Start is called once when the game begins (after Awake)
    // Finds and stores reference to the player GameObject
    private void Start()
    {
        // Find the GameObject named "Player" in the scene
        // GameObject.Find searches the entire scene by name
        // NOTE: This assumes the player GameObject is named exactly "Player"
        player = GameObject.Find("Player");
    }

    // ==================================================
    // ON TRIGGER ENTER METHOD
    // ==================================================
    // Called automatically by Unity when another collider enters this trigger
    // Checks if the player reached the mission end point
    //
    // PARAMETERS:
    // Collider other - The collider that entered the trigger
    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is NOT the player
        // If it's not the player (e.g. enemy, object), exit early (do nothing)
        if (other.gameObject != player) return;

        // Player has reached the mission end point!
        // Print success message to console (for debugging)
        Debug.Log("Du hast es geschafft du 1 gluk");

        // TODO: Implement mission completion logic
        // Examples:
        // - Show victory screen
        // - Load next level
        // - Award experience/rewards
        // - Display mission statistics
        //Ende
    }
}