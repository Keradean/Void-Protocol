/*
====================================================================
* AudioPreviewDebug.cs - Editor Audio Testing Tool v3.0
====================================================================
* Project: Space Colony Game
* Course: PIP
* Script-Developer: Julian
* Date: 28.09.2025
* Version: v3.0 - Konflikt-Bereinigt
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
* AUTHORSHIP CLASSIFICATION:
* [HUMAN-AUTHORED] - Menu structure concept, audio testing workflow
* [AI-ASSISTED] - Conflict resolution, error handling optimization, editor integration
* 
* BEREINIGUNGSNOTIZEN v3.0:
* - Debug-Output Elimination: Produktive Debug.Log Statements entfernt
* - Editor-Integration: Robuste MenuItem ohne Runtime-Konflikte
* - Error-Handling: Graceful degradation bei missing AudioClip Selection
====================================================================
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class AudioPreviewDebug
{
    // Configuration constants - conflict-free implementation
    private const float DEFAULT_START_TIME = 0.5f;
    private const float DEFAULT_END_TIME = 1.5f;
    private const string MENU_PATH = "Tools/Audio Preview/";

    [MenuItem(MENU_PATH + "Toggle Selected Segment (0.5s..1.5s)")]
    public static void ToggleSelectedSegment()
    {
        var clip = Selection.activeObject as AudioClip;
        if (!ValidateAudioClipSelection(clip)) return;

        float startTime = DEFAULT_START_TIME;
        float endTime = Mathf.Min(DEFAULT_END_TIME, clip.length);

        // Conflict-free audio preview call
        AudioPreviewUtility.ToggleSegment(clip, startTime, endTime);
    }

    [MenuItem(MENU_PATH + "Stop Selected")]
    public static void StopSelected()
    {
        var clip = Selection.activeObject as AudioClip;
        if (!ValidateAudioClipSelection(clip)) return;

        // Conflict-free stop call
        AudioPreviewUtility.Stop(clip);
    }

    [MenuItem(MENU_PATH + "Test Audio System Status")]
    public static void TestAudioSystemStatus()
    {
        var clip = Selection.activeObject as AudioClip;
        if (!ValidateAudioClipSelection(clip)) return;

        // System status check without debug pollution
        bool isPlaying = AudioPreviewUtility.IsPlaying(clip);
        string status = isPlaying ? "Playing" : "Stopped";

        EditorUtility.DisplayDialog(
            "Audio Preview Status",
            $"Audio System Status: {status}\nSelected Clip: {clip.name}\nLength: {clip.length:F2}s",
            "OK"
        );
    }

    // Validation methods - conflict-free implementation
    [MenuItem(MENU_PATH + "Toggle Selected Segment (0.5s..1.5s)", true)]
    [MenuItem(MENU_PATH + "Stop Selected", true)]
    [MenuItem(MENU_PATH + "Test Audio System Status", true)]
    public static bool ValidateAudioClipMenuItems()
    {
        return Selection.activeObject is AudioClip;
    }

    private static bool ValidateAudioClipSelection(AudioClip clip)
    {
        if (clip == null)
        {
            EditorUtility.DisplayDialog(
                "Audio Preview",
                "Please select an AudioClip in the Project window first.",
                "OK"
            );
            return false;
        }

        if (clip.length <= 0f)
        {
            EditorUtility.DisplayDialog(
                "Audio Preview",
                "Selected AudioClip has invalid length.",
                "OK"
            );
            return false;
        }

        return true;
    }
}
#endif