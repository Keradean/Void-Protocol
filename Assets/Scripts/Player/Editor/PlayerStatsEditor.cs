using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(PlayerStats))]
public class PlayerStatsEditor : Editor
{
    private PlayerStats StatsTarget => target as PlayerStats; // Cast the target to PlayerStats for easy access
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Draw the default inspector GUI
        if (GUILayout.Button("Reset Stats"))// Create a button in the inspector
        {
            StatsTarget.ResetStats(); // Call the ResetStats method when the button is clicked
            EditorUtility.SetDirty(StatsTarget); // Mark the target as dirty to ensure changes are saved
        }
    }
}
