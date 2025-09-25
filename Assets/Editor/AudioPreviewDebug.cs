using UnityEditor;
using UnityEngine;

public static class AudioPreviewDebug
{
    [MenuItem("Tools/Audio Preview/Toggle Selected Segment (0.5s..1.5s)")]
    public static void ToggleSelectedSegment()
    {
        var clip = Selection.activeObject as AudioClip;
        if (clip == null)
        {
            Debug.Log("Select an AudioClip in the Project window first.");
            return;
        }
        float start = 0.5f;
        float end = Mathf.Min(1.5f, clip.length);
        AudioPreviewUtility.ToggleSegment(clip, start, end);
    }

    [MenuItem("Tools/Audio Preview/Stop Selected")]
    public static void StopSelected()
    {
        var clip = Selection.activeObject as AudioClip;
        if (clip == null)
        {
            Debug.Log("Select an AudioClip in the Project window first.");
            return;
        }
        AudioPreviewUtility.Stop(clip);
    }
}
