/*
====================================================================
Waypoint
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
// WAYPOINT CLASS
// ==================================================
// This class defines a waypoint system for entities (enemies, NPCs, vehicles, etc.)
// It stores multiple points (positions) that an entity can move between
// This is commonly used for patrol routes, paths, or predefined movement patterns
//
// HOW IT WORKS:
// - The waypoint object is placed in the scene at a base position
// - Points are defined as offsets from this base position (relative positions)
// - Entities can request waypoint positions and move between them
public class Waypoint : MonoBehaviour
{
    // ==================================================
    // VARIABLE DECLARATION
    // ==================================================

    [Header("Config")] // Header in Unity Inspector for organization
    // Array of waypoint positions (stored as Vector3 offsets)
    // These are RELATIVE positions from the EntityPosition (not absolute world positions)
    // Example: points[0] = (5, 0, 0) means "5 units to the right of the base position"
    [SerializeField] private Vector3[] points;

    // Public property to access the points array from other scripts
    // Read-only - other scripts can see the points but not change them directly
    public Vector3[] Points => points;

    // The base position of the waypoint system (usually the position of this GameObject)
    // { get; set; } allows other scripts to read AND write this position
    // This can be updated if the waypoint system needs to move during gameplay
    public Vector3 EntityPosition { get; set; }

    // Boolean to track if the game has started (currently not used in the code)
    private bool gameStarted;

    // ==================================================
    // START METHOD
    // ==================================================
    // Start is called once when the game begins (after Awake)
    // Initializes the base position for the waypoint system
    private void Start()
    {
        // Set the EntityPosition to this GameObject's world position
        // This becomes the "origin point" from which all waypoint offsets are calculated
        EntityPosition = transform.position;
    }

    // ==================================================
    // GET POSITION METHOD
    // ==================================================
    // This public method returns the absolute world position of a specific waypoint
    // It calculates: base position + offset = final world position
    //
    // PARAMETERS:
    // int pointIndex - The index of the waypoint in the points array (0, 1, 2, etc.)
    //
    // RETURNS:
    // Vector3 - The absolute world position of the requested waypoint
    //
    // USAGE EXAMPLE:
    // Vector3 targetPos = waypoint.GetPosition(0); // Get first waypoint's world position
    // agent.SetDestination(targetPos); // Move AI to that position
    public Vector3 GetPosition(int pointIndex)
    {
        // Add the EntityPosition (base) to the point offset to get absolute world position
        // Example: EntityPosition = (10, 0, 10), points[0] = (5, 0, 0)
        // Result: (10, 0, 10) + (5, 0, 0) = (15, 0, 10) in world space
        return EntityPosition + points[pointIndex];
    }

    // ==================================================
    // ON DRAW GIZMOS METHOD
    // ==================================================
    // This method is called by Unity Editor to draw visual debugging aids in the Scene view
    // It draws colored spheres at each waypoint position so you can see the path in the editor
    // This ONLY runs in the Unity Editor, not in the built game
    private void OnDrawGizmos()
    {
        // Safety check: if no points are defined or array is empty, don't draw anything
        if (Points == null || Points.Length == 0) return;

        // Set the color for the gizmos (red spheres)
        Gizmos.color = Color.red;

        // Loop through all waypoint positions
        foreach (var point in Points)
        {
            // Draw a red sphere at each waypoint's world position
            // EntityPosition + point = absolute position
            // 0.2f is the radius of the sphere (small sphere for visibility)
            Gizmos.DrawSphere(EntityPosition + point, 0.2f);
        }
    }
}