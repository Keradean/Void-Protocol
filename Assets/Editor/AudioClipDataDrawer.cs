using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AudioClipData))]
public class AudioClipDataDrawer : PropertyDrawer
{
    // Layout values (as requested)
    private const float TIMELINE_HEIGHT = 10f;
    private const float BUTTON_HEIGHT = 20f;
    private const float LINE_HEIGHT = 55f;
    private const float PADDING = 4f;
    private const float HANDLE_HALF_WIDTH = 6f; // used for hitbox width
    private const float FIELD_HEIGHT = 18f; // compact property field height

    // Drag state
    private bool isDraggingStart = false;
    private bool isDraggingEnd = false;
    private bool isDraggingSelection = false;
    private float dragSelectionOffsetTime = 0f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Find properties
        var clipProp = property.FindPropertyRelative("clip");
        var volumeProp = property.FindPropertyRelative("volume");
        var pitchProp = property.FindPropertyRelative("pitch");
        var is3DProp = property.FindPropertyRelative("is3D");
        var loopProp = property.FindPropertyRelative("loop");
        var startTimeProp = property.FindPropertyRelative("startTime");
        var endTimeProp = property.FindPropertyRelative("endTime");
        var fadeInProp = property.FindPropertyRelative("fadeInDuration");
        var fadeOutProp = property.FindPropertyRelative("fadeOutDuration");

        float currentY = position.y;

        // Foldout
        var headerRect = new Rect(position.x, currentY, position.width, LINE_HEIGHT);
        property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, true);
        currentY += LINE_HEIGHT + PADDING;

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            // Clip field
            var clipRect = new Rect(position.x, currentY, position.width, LINE_HEIGHT);
            EditorGUI.PropertyField(clipRect, clipProp);
            currentY += LINE_HEIGHT + PADDING;

            var clip = clipProp.objectReferenceValue as AudioClip;

            if (clip != null)
            {
                // Track length info
                var lengthRect = new Rect(position.x, currentY, position.width, LINE_HEIGHT);
                EditorGUI.HelpBox(lengthRect, "Track Length: " + clip.length.ToString("F2") + " s", MessageType.None);
                currentY += LINE_HEIGHT + PADDING;

                // Clamp and normalize times
                float startTime = startTimeProp.floatValue;
                float endTime = endTimeProp.floatValue <= 0f ? clip.length : endTimeProp.floatValue;

                startTime = Mathf.Clamp(startTime, 0f, clip.length);
                endTime = Mathf.Clamp(endTime, startTime, clip.length);

                startTimeProp.floatValue = startTime;
                endTimeProp.floatValue = endTime;

                // Timeline
                var timelineRect = new Rect(position.x, currentY, position.width, TIMELINE_HEIGHT);
                DrawTimeline(timelineRect, clip, startTimeProp, endTimeProp);
                currentY += TIMELINE_HEIGHT + PADDING;

                // Selection info
                var selRect = new Rect(position.x, currentY, position.width, LINE_HEIGHT);
                float duration = endTime - startTime;
                EditorGUI.HelpBox(selRect, "Selected: " + startTime.ToString("F2") + "s - " + endTime.ToString("F2") + "s (Duration: " + duration.ToString("F2") + "s)", MessageType.None);
                currentY += LINE_HEIGHT + PADDING;

                // Preview buttons (Editor preview; disabled in Play Mode)
                var controlsRect = new Rect(position.x, currentY, position.width, BUTTON_HEIGHT);
                DrawPreviewControls(controlsRect, clip, startTime, endTime, volumeProp.floatValue, pitchProp.floatValue);
                currentY += BUTTON_HEIGHT + PADDING;
            }

            // Compact props
            currentY = DrawCompactProperties(position, currentY, volumeProp, pitchProp, is3DProp, loopProp, fadeInProp, fadeOutProp);

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private void DrawTimeline(Rect rect, AudioClip clip, SerializedProperty startTimeProp, SerializedProperty endTimeProp)
    {
        if (clip == null || clip.length <= 0f) return;

        Event e = Event.current;
        float clipLength = clip.length;

        float startTime = startTimeProp.floatValue;
        float endTime = endTimeProp.floatValue <= 0f ? clipLength : endTimeProp.floatValue;

        // Background
        EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f, 1f));

        // Selection bar (thin)
        float startX = rect.x + (startTime / clipLength) * rect.width;
        float endX = rect.x + (endTime / clipLength) * rect.width;
        float selX = Mathf.Min(startX, endX);
        float selW = Mathf.Max(0f, endX - startX);
        var selectionRect = new Rect(selX, rect.y + 1f, selW, Mathf.Max(1f, rect.height - 2f));
        EditorGUI.DrawRect(selectionRect, new Color(0.2f, 0.6f, 0.2f, 0.5f));

        // Hitboxes for handles (still rectangular for detection)
        Rect startHit = new Rect(startX - HANDLE_HALF_WIDTH, rect.y - 4f, HANDLE_HALF_WIDTH * 2f, rect.height + 8f);
        Rect endHit = new Rect(endX - HANDLE_HALF_WIDTH, rect.y - 4f, HANDLE_HALF_WIDTH * 2f, rect.height + 8f);

        // Cursor hints
        if (startHit.Contains(e.mousePosition))
            EditorGUIUtility.AddCursorRect(startHit, MouseCursor.ResizeHorizontal);
        if (endHit.Contains(e.mousePosition))
            EditorGUIUtility.AddCursorRect(endHit, MouseCursor.ResizeHorizontal);
        if (selectionRect.Contains(e.mousePosition))
            EditorGUIUtility.AddCursorRect(selectionRect, MouseCursor.MoveArrow);

        // Mouse handling
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (startHit.Contains(e.mousePosition))
                    {
                        isDraggingStart = true;
                        e.Use();
                    }
                    else if (endHit.Contains(e.mousePosition))
                    {
                        isDraggingEnd = true;
                        e.Use();
                    }
                    else if (selectionRect.Contains(e.mousePosition))
                    {
                        isDraggingSelection = true;
                        float clickTime = Mathf.Clamp((e.mousePosition.x - rect.x) / rect.width * clipLength, 0f, clipLength);
                        dragSelectionOffsetTime = clickTime - startTime;
                        e.Use();
                    }
                }
                break;

            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    if (isDraggingStart)
                    {
                        float newStart = Mathf.Clamp((e.mousePosition.x - rect.x) / rect.width * clipLength, 0f, endTime);
                        if (!Mathf.Approximately(newStart, startTime))
                        {
                            startTimeProp.floatValue = newStart;
                            startTime = newStart;
                            startTimeProp.serializedObject.ApplyModifiedProperties();
                            GUI.changed = true;
                        }
                        e.Use();
                    }
                    else if (isDraggingEnd)
                    {
                        float newEnd = Mathf.Clamp((e.mousePosition.x - rect.x) / rect.width * clipLength, startTime, clipLength);
                        if (!Mathf.Approximately(newEnd, endTime))
                        {
                            endTimeProp.floatValue = newEnd;
                            endTime = newEnd;
                            endTimeProp.serializedObject.ApplyModifiedProperties();
                            GUI.changed = true;
                        }
                        e.Use();
                    }
                    else if (isDraggingSelection)
                    {
                        float clickTime = Mathf.Clamp((e.mousePosition.x - rect.x) / rect.width * clipLength, 0f, clipLength);
                        float duration = endTime - startTime;
                        float newStart = Mathf.Clamp(clickTime - dragSelectionOffsetTime, 0f, clipLength - duration);
                        float newEnd = newStart + duration;

                        if (!Mathf.Approximately(newStart, startTime) || !Mathf.Approximately(newEnd, endTime))
                        {
                            startTimeProp.floatValue = newStart;
                            endTimeProp.floatValue = newEnd;
                            startTime = newStart;
                            endTime = newEnd;
                            startTimeProp.serializedObject.ApplyModifiedProperties();
                            GUI.changed = true;
                        }
                        e.Use();
                    }
                }
                break;

            case EventType.MouseUp:
                if (e.button == 0 && (isDraggingStart || isDraggingEnd || isDraggingSelection))
                {
                    isDraggingStart = false;
                    isDraggingEnd = false;
                    isDraggingSelection = false;
                    e.Use();
                }
                break;
        }

        // Draw round handles last (modern UI)
        // Use Handles in GUI to draw small discs
        Handles.BeginGUI();
        {
            // start circle
            var startCenter = new Vector3(startX, rect.y + rect.height * 0.5f, 0f);
            Handles.color = Color.green;
            Handles.DrawSolidDisc(startCenter, Vector3.forward, 4f);

            // end circle
            var endCenter = new Vector3(endX, rect.y + rect.height * 0.5f, 0f);
            Handles.color = Color.red;
            Handles.DrawSolidDisc(endCenter, Vector3.forward, 4f);
        }
        Handles.EndGUI();
        Handles.color = Color.white;
    }

    private void DrawPreviewControls(Rect rect, AudioClip clip, float startTime, float endTime, float volume, float pitch)
    {
        // Single centered Play/Stop Segment button
        string label = (clip != null && AudioPreviewUtility.IsPlaying(clip)) ? "Stop Segment" : "Play Segment";

        float w = 200f;
        float x = rect.x + (rect.width - w) * 0.5f;
        Rect btn = new Rect(x, rect.y, w, rect.height);

        bool prevEnabled = GUI.enabled;
        GUI.enabled = !Application.isPlaying && clip != null; // disable in Play Mode

        if (GUI.Button(btn, label))
        {
            AudioPreviewUtility.ToggleSegment(clip, startTime, endTime);
        }

        GUI.enabled = prevEnabled;
    }




    private float DrawCompactProperties(Rect position, float currentY,
    SerializedProperty volumeProp, SerializedProperty pitchProp,
    SerializedProperty is3DProp, SerializedProperty loopProp,
    SerializedProperty fadeInProp, SerializedProperty fadeOutProp)
    {
        float halfWidth = (position.width - PADDING) * 0.5f;

        // Row 1: Volume | Pitch
        var volumeRect = new Rect(position.x, currentY, halfWidth, FIELD_HEIGHT);
        var pitchRect = new Rect(position.x + halfWidth + PADDING, currentY, halfWidth, FIELD_HEIGHT);
        EditorGUI.PropertyField(volumeRect, volumeProp);
        EditorGUI.PropertyField(pitchRect, pitchProp);
        currentY += FIELD_HEIGHT + PADDING;

        // Row 2: Is3D | Loop
        var is3DRect = new Rect(position.x, currentY, halfWidth, FIELD_HEIGHT);
        var loopRect = new Rect(position.x + halfWidth + PADDING, currentY, halfWidth, FIELD_HEIGHT);
        EditorGUI.PropertyField(is3DRect, is3DProp);
        EditorGUI.PropertyField(loopRect, loopProp);
        currentY += FIELD_HEIGHT + PADDING;

        // Row 3: Fade In | Fade Out
        var fadeInRect = new Rect(position.x, currentY, halfWidth, FIELD_HEIGHT);
        var fadeOutRect = new Rect(position.x + halfWidth + PADDING, currentY, halfWidth, FIELD_HEIGHT);
        EditorGUI.PropertyField(fadeInRect, fadeInProp);
        EditorGUI.PropertyField(fadeOutRect, fadeOutProp);
        currentY += FIELD_HEIGHT + PADDING;

        return currentY;
    }


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return LINE_HEIGHT;

        float height = 0f;
        height += LINE_HEIGHT + PADDING;      // header
        height += LINE_HEIGHT + PADDING;      // clip field

        var clipProp = property.FindPropertyRelative("clip");
        if (clipProp.objectReferenceValue != null)
        {
            height += LINE_HEIGHT + PADDING;      // track length
            height += TIMELINE_HEIGHT + PADDING;  // timeline
            height += LINE_HEIGHT + PADDING;      // selection info
            height += BUTTON_HEIGHT + PADDING;    // preview controls
        }

        // Use FIELD_HEIGHT for compact two-column rows (Volume|Pitch, Is3D|Loop, FadeIn|FadeOut)
        height += (FIELD_HEIGHT + PADDING) * 3f;

        return height;
    }

}
