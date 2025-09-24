/*
====================================================================
* AudioClipDataDrawer.cs - Visual Timeline Audio Editor
====================================================================
* Project: Space Colony Game
* Course: PIP
* Script-Developer: Julian
* Date: 24.09.2025
* Version: v1.0 - Custom Timeline Audio Segment Selection
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
* AUTHORSHIP CLASSIFICATION:
* [HUMAN-AUTHORED] - Audio segment selection concept, timeline UI requirements
* [AI-ASSISTED] - Unity PropertyDrawer implementation, timeline visualization
====================================================================
*/

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AudioClipData))]
public class AudioClipDataDrawer : PropertyDrawer
{
    private const float LINE_HEIGHT = 18f;
    private const float SPACING = 2f;
    private const float TIMELINE_HEIGHT = 40f;
    private const float HANDLE_WIDTH = 8f;

    // Colors for timeline visualization - Added by Julian with AI-Support
    private static readonly Color TIMELINE_BG = new Color(0.3f, 0.3f, 0.3f, 1f);
    private static readonly Color SELECTED_SEGMENT = new Color(0.2f, 0.8f, 0.2f, 0.6f);
    private static readonly Color UNSELECTED_SEGMENT = new Color(0.6f, 0.6f, 0.6f, 0.3f);
    private static readonly Color HANDLE_COLOR = new Color(0.8f, 0.8f, 0.2f, 1f);
    private static readonly Color TEXT_COLOR = Color.white;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Get properties
        var clipProp = property.FindPropertyRelative("clip");
        var volumeProp = property.FindPropertyRelative("volume");
        var pitchProp = property.FindPropertyRelative("pitch");
        var is3DProp = property.FindPropertyRelative("is3D");
        var loopProp = property.FindPropertyRelative("loop");
        var startTimeProp = property.FindPropertyRelative("startTime");
        var endTimeProp = property.FindPropertyRelative("endTime");
        var fadeInProp = property.FindPropertyRelative("fadeInDuration");
        var fadeOutProp = property.FindPropertyRelative("fadeOutDuration");

        // Calculate positions
        Rect currentRect = position;
        currentRect.height = LINE_HEIGHT;

        // Draw foldout header with clip info
        bool isExpanded = property.isExpanded;
        AudioClip audioClip = clipProp.objectReferenceValue as AudioClip;

        string headerText = label.text;
        if (audioClip != null)
        {
            headerText += $" ({audioClip.name} - {audioClip.length:F1}s)";
        }

        property.isExpanded = EditorGUI.Foldout(currentRect, isExpanded, headerText, true);
        currentRect.y += LINE_HEIGHT + SPACING;

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            // Audio Clip field
            EditorGUI.PropertyField(currentRect, clipProp);
            currentRect.y += LINE_HEIGHT + SPACING;

            if (audioClip != null)
            {
                // TIMELINE EDITOR SECTION - Added by Julian with AI-Support
                DrawTimelineEditor(currentRect, audioClip, startTimeProp, endTimeProp);
                currentRect.y += TIMELINE_HEIGHT + SPACING * 3;

                // Audio segment info display
                float startTime = startTimeProp.floatValue;
                float endTime = endTimeProp.floatValue;
                if (endTime <= 0f) endTime = audioClip.length;

                float segmentDuration = endTime - startTime;

                EditorGUI.LabelField(currentRect, $"Selected Segment: {startTime:F2}s - {endTime:F2}s (Duration: {segmentDuration:F2}s)",
                    EditorStyles.helpBox);
                currentRect.y += LINE_HEIGHT + SPACING;

                // Preview buttons
                Rect buttonRect = currentRect;
                buttonRect.width = 80f;

                if (GUI.Button(buttonRect, "Play Full"))
                {
                    PlayAudioClip(audioClip, 0f, audioClip.length);
                }

                buttonRect.x += 85f;
                if (GUI.Button(buttonRect, "Play Segment"))
                {
                    PlayAudioClip(audioClip, startTime, endTime);
                }

                buttonRect.x += 85f;
                if (GUI.Button(buttonRect, "Stop"))
                {
                    StopAllClips();
                }

                currentRect.y += LINE_HEIGHT + SPACING * 2;
            }

            // Standard audio properties
            DrawAudioProperties(ref currentRect, volumeProp, pitchProp, is3DProp, loopProp, fadeInProp, fadeOutProp);

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private void DrawTimelineEditor(Rect rect, AudioClip clip, SerializedProperty startProp, SerializedProperty endProp)
    {
        if (clip == null) return;

        float clipLength = clip.length;
        float startTime = startProp.floatValue;
        float endTime = endProp.floatValue;

        if (endTime <= 0f) endTime = clipLength;

        // Ensure valid range
        startTime = Mathf.Clamp(startTime, 0f, clipLength);
        endTime = Mathf.Clamp(endTime, startTime, clipLength);

        // Timeline background
        Rect timelineRect = new Rect(rect.x, rect.y, rect.width, TIMELINE_HEIGHT);
        EditorGUI.DrawRect(timelineRect, TIMELINE_BG);

        // Draw time markers
        DrawTimeMarkers(timelineRect, clipLength);

        // Draw audio segments
        DrawAudioSegments(timelineRect, clipLength, startTime, endTime);

        // Handle user input for dragging
        HandleTimelineInput(timelineRect, clipLength, startProp, endProp);

        // Draw handles
        DrawTimelineHandles(timelineRect, clipLength, startTime, endTime);
    }

    private void DrawTimeMarkers(Rect timelineRect, float clipLength)
    {
        // Draw time markers every 0.5 seconds or appropriate interval
        float interval = GetTimeInterval(clipLength);
        int markerCount = Mathf.FloorToInt(clipLength / interval) + 1;

        for (int i = 0; i <= markerCount; i++)
        {
            float time = i * interval;
            if (time > clipLength) time = clipLength;

            float x = timelineRect.x + (time / clipLength) * timelineRect.width;

            // Draw marker line
            Rect markerRect = new Rect(x - 0.5f, timelineRect.y, 1f, timelineRect.height);
            EditorGUI.DrawRect(markerRect, Color.gray);

            // Draw time label
            if (i % 2 == 0 || clipLength < 2f) // Show every other marker for longer clips
            {
                GUIContent timeLabel = new GUIContent($"{time:F1}s");
                Vector2 labelSize = GUI.skin.label.CalcSize(timeLabel);

                Rect labelRect = new Rect(x - labelSize.x * 0.5f, timelineRect.y - 15f, labelSize.x, 15f);
                GUI.Label(labelRect, timeLabel, EditorStyles.miniLabel);
            }
        }
    }

    private float GetTimeInterval(float clipLength)
    {
        if (clipLength <= 1f) return 0.1f;
        if (clipLength <= 5f) return 0.5f;
        if (clipLength <= 20f) return 1f;
        return 2f;
    }

    private void DrawAudioSegments(Rect timelineRect, float clipLength, float startTime, float endTime)
    {
        float startX = timelineRect.x + (startTime / clipLength) * timelineRect.width;
        float endX = timelineRect.x + (endTime / clipLength) * timelineRect.width;

        // Unselected segments
        if (startTime > 0f)
        {
            Rect leftSegment = new Rect(timelineRect.x, timelineRect.y, startX - timelineRect.x, timelineRect.height);
            EditorGUI.DrawRect(leftSegment, UNSELECTED_SEGMENT);
        }

        if (endTime < clipLength)
        {
            Rect rightSegment = new Rect(endX, timelineRect.y, timelineRect.xMax - endX, timelineRect.height);
            EditorGUI.DrawRect(rightSegment, UNSELECTED_SEGMENT);
        }

        // Selected segment
        Rect selectedSegment = new Rect(startX, timelineRect.y, endX - startX, timelineRect.height);
        EditorGUI.DrawRect(selectedSegment, SELECTED_SEGMENT);

        // Selected segment border
        Rect borderRect = new Rect(startX, timelineRect.y, endX - startX, 2f);
        EditorGUI.DrawRect(borderRect, HANDLE_COLOR);
        borderRect.y = timelineRect.yMax - 2f;
        EditorGUI.DrawRect(borderRect, HANDLE_COLOR);
    }

    private void DrawTimelineHandles(Rect timelineRect, float clipLength, float startTime, float endTime)
    {
        float startX = timelineRect.x + (startTime / clipLength) * timelineRect.width;
        float endX = timelineRect.x + (endTime / clipLength) * timelineRect.width;

        // Start handle
        Rect startHandle = new Rect(startX - HANDLE_WIDTH * 0.5f, timelineRect.y, HANDLE_WIDTH, timelineRect.height);
        EditorGUI.DrawRect(startHandle, HANDLE_COLOR);

        // End handle
        Rect endHandle = new Rect(endX - HANDLE_WIDTH * 0.5f, timelineRect.y, HANDLE_WIDTH, timelineRect.height);
        EditorGUI.DrawRect(endHandle, HANDLE_COLOR);

        // Handle labels
        GUI.Label(new Rect(startHandle.x, startHandle.yMax + 2f, HANDLE_WIDTH * 2f, 15f), "S", EditorStyles.miniLabel);
        GUI.Label(new Rect(endHandle.x, endHandle.yMax + 2f, HANDLE_WIDTH * 2f, 15f), "E", EditorStyles.miniLabel);
    }

    private void HandleTimelineInput(Rect timelineRect, float clipLength, SerializedProperty startProp, SerializedProperty endProp)
    {
        Event evt = Event.current;
        if (evt.type == EventType.MouseDown && timelineRect.Contains(evt.mousePosition))
        {
            float clickTime = ((evt.mousePosition.x - timelineRect.x) / timelineRect.width) * clipLength;
            clickTime = Mathf.Clamp(clickTime, 0f, clipLength);

            float startTime = startProp.floatValue;
            float endTime = endProp.floatValue;
            if (endTime <= 0f) endTime = clipLength;

            // Determine which handle to move based on proximity
            float startDistance = Mathf.Abs(clickTime - startTime);
            float endDistance = Mathf.Abs(clickTime - endTime);

            if (startDistance < endDistance)
            {
                // Move start handle
                startProp.floatValue = Mathf.Min(clickTime, endTime - 0.1f);
            }
            else
            {
                // Move end handle
                endProp.floatValue = Mathf.Max(clickTime, startTime + 0.1f);
            }

            evt.Use();
            GUI.changed = true;
        }
    }

    private void DrawAudioProperties(ref Rect currentRect, SerializedProperty volumeProp, SerializedProperty pitchProp,
        SerializedProperty is3DProp, SerializedProperty loopProp, SerializedProperty fadeInProp, SerializedProperty fadeOutProp)
    {
        // Volume and Pitch on same line
        Rect leftRect = new Rect(currentRect.x, currentRect.y, currentRect.width * 0.5f - 5f, LINE_HEIGHT);
        Rect rightRect = new Rect(currentRect.x + currentRect.width * 0.5f, currentRect.y, currentRect.width * 0.5f, LINE_HEIGHT);

        EditorGUI.PropertyField(leftRect, volumeProp);
        EditorGUI.PropertyField(rightRect, pitchProp);
        currentRect.y += LINE_HEIGHT + SPACING;

        // Boolean properties on same line
        leftRect.y = currentRect.y;
        rightRect.y = currentRect.y;

        EditorGUI.PropertyField(leftRect, is3DProp);
        EditorGUI.PropertyField(rightRect, loopProp);
        currentRect.y += LINE_HEIGHT + SPACING;

        // Fade properties
        EditorGUI.PropertyField(currentRect, fadeInProp);
        currentRect.y += LINE_HEIGHT + SPACING;

        EditorGUI.PropertyField(currentRect, fadeOutProp);
        currentRect.y += LINE_HEIGHT + SPACING;
    }

    private void PlayAudioClip(AudioClip clip, float startTime, float endTime)
    {
        if (clip == null) return;

        // Use Unity's internal audio preview (Editor only)
        System.Type audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
        if (audioUtilType != null)
        {
            var playMethod = audioUtilType.GetMethod("PlayPreviewClip",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            if (playMethod != null)
            {
                // For now, play full clip (custom segment playback requires more complex implementation)
                playMethod.Invoke(null, new object[] { clip, (int)(startTime * clip.frequency), false });
            }
        }
    }

    private void StopAllClips()
    {
        System.Type audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
        if (audioUtilType != null)
        {
            var stopMethod = audioUtilType.GetMethod("StopAllPreviewClips",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            if (stopMethod != null)
            {
                stopMethod.Invoke(null, null);
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return LINE_HEIGHT;

        var clipProp = property.FindPropertyRelative("clip");
        AudioClip audioClip = clipProp.objectReferenceValue as AudioClip;

        float height = LINE_HEIGHT + SPACING; // Foldout
        height += LINE_HEIGHT + SPACING; // Clip field

        if (audioClip != null)
        {
            height += TIMELINE_HEIGHT + SPACING * 3; // Timeline
            height += LINE_HEIGHT + SPACING; // Segment info
            height += LINE_HEIGHT + SPACING * 2; // Preview buttons
        }

        height += (LINE_HEIGHT + SPACING) * 5; // Standard properties (volume, pitch, is3D, loop, fade in/out)

        return height;
    }
}
#endif