/*
====================================================================
WaypointEditor
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
using UnityEditor;
using UnityEngine;

// ==================================================
// WAYPOINT EDITOR CLASS
// ==================================================
// This is a CUSTOM EDITOR script that extends Unity's Inspector functionality
// It allows you to visually edit waypoint positions directly in the Scene view
// Instead of typing numbers in the Inspector, you can drag handles in the scene
//
// IMPORTANT: This script ONLY works in the Unity Editor, not in built games
// It's a development tool to make waypoint creation easier for level designers
//
// [CustomEditor(typeof(Waypoint))] tells Unity:
// "Use this custom editor when a Waypoint component is selected in the Inspector"
[CustomEditor(typeof(Waypoint))]
public class WaypointEditor : Editor
{
    // ==================================================
    // WAYPOINT TARGET PROPERTY
    // ==================================================
    // This property gets the Waypoint component that is currently being edited
    // "target" is provided by Unity's Editor class - it's the selected object
    // "as Waypoint" casts the target to Waypoint type (type conversion)
    // This allows us to access Waypoint-specific properties and methods
    private Waypoint WaypointTarget => target as Waypoint;

    // ==================================================
    // ON SCENE GUI METHOD
    // ==================================================
    // This method is called by Unity Editor to draw custom GUI elements in the Scene view
    // It's where we draw the interactive handles that allow dragging waypoints
    // Think of it like Update(), but for the Unity Editor's Scene view
    private void OnSceneGUI()
    {
        // Safety check: if there are no waypoints defined, don't draw anything
        if (WaypointTarget.Points.Length <= 0) return;

        // Set the color for the handles (red, matching the gizmos in Waypoint.cs)
        Handles.color = Color.red;

        // Loop through all waypoint points
        for (int i = 0; i < WaypointTarget.Points.Length; i++)
        {
            // Begin tracking changes to the handle (for Undo system)
            // This allows Unity to detect when the user moves a handle
            EditorGUI.BeginChangeCheck();

            // Calculate the current world position of this waypoint
            // EntityPosition (base) + Points[i] (offset) = absolute world position
            Vector3 currentPoint = WaypointTarget.EntityPosition + WaypointTarget.Points[i];

            // Draw a position handle at the current waypoint position
            // Handles.PositionHandle creates 3D arrows that users can drag to move the point
            // Quaternion.identity means no rotation (standard XYZ axes)
            // Returns the new position if the user drags the handle
            Vector3 newPosition = Handles.PositionHandle(currentPoint, Quaternion.identity);

            // ==================================================
            // CREATE LABEL STYLE
            // ==================================================
            // Create a custom text style for the waypoint labels
            GUIStyle text = new GUIStyle();
            text.fontStyle = FontStyle.Bold; // Make text bold
            text.fontSize = 18; // Larger font size for visibility
            text.normal.textColor = Color.black; // Black text color

            // Offset for the label position (slightly to the right and down from the waypoint)
            // This prevents the label from overlapping with the handle
            Vector3 textPos = new Vector3(0.2f, -0.2f);

            // Draw a label showing the waypoint number (1-based index for user-friendliness)
            // Example: First waypoint shows "1", second shows "2", etc.
            // $"{i + 1}" uses string interpolation (i starts at 0, so we add 1 for display)
            Handles.Label(WaypointTarget.EntityPosition + WaypointTarget.Points[i] + textPos, $"{i + 1}", text);

            // ==================================================
            // CHECK IF HANDLE WAS MOVED
            // ==================================================
            // EndChangeCheck returns true if the user moved the handle
            if (EditorGUI.EndChangeCheck())
            {
                // Record this change in Unity's Undo system
                // This allows users to press Ctrl+Z to undo waypoint movements
                // "Free Move" is the name that appears in the Undo history
                Undo.RecordObject(target, "Free Move");

                // Update the waypoint's position with the new position
                // Convert from world position back to offset (relative position)
                // newPosition (world) - EntityPosition (base) = offset
                // Example: newPosition = (15, 0, 10), EntityPosition = (10, 0, 10)
                // Result: (15, 0, 10) - (10, 0, 10) = (5, 0, 0) offset
                WaypointTarget.Points[i] = newPosition - WaypointTarget.EntityPosition;
            }
        }
    }
}