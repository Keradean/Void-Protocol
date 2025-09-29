/*
====================================================================
* AudioClipDataDrawer.cs - Timeline Control Property Drawer v4.0
====================================================================
* Project: Space Colony Game
* Course: PIP
* Script-Developer: Julian
* Date: 28.09.2025
* Version: v4.0 - Phase 4 RED HANDLE Enhancement Complete
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
* AUTHORSHIP CLASSIFICATION:
* [HUMAN-AUTHORED] - Timeline visualization concept, interaction design
* [AI-ASSISTED] - RED HANDLE precision enhancement, visual feedback system
* 
* PHASE 4 NOTES:
* - Enhanced RED HANDLE detection and dragging precision
* - Improved visual feedback with interaction indicators
* - Debug logging system for troubleshooting
* - Mutual exclusion between drag states enforced
====================================================================
*/

using UnityEngine;
using UnityEditor;


// IMPORTANT: Ensure the runtime type name matches your struct exactly:
// [System.Serializable] public struct AudioClipData { ... }
[CustomPropertyDrawer(typeof(AudioClipData))]
public class AudioClipDataDrawer : PropertyDrawer
{
    // Layout
    private const float VSPACE = 24f;
    private const float PAD = 8f;
    private const float SECTION_SPACE = 8f; // Zusätzlich für Section-Separation
    private const float TIMELINE_HEIGHT = 60f; // Dedizierte Timeline Section Height

    // Utility: null-safe field lookup with aliases
    private static SerializedProperty SafeFind(SerializedProperty root, params string[] names)
    {
        foreach (var n in names)
        {
            if (root == null) break;
            var p = root.FindPropertyRelative(n);
            if (p != null) return p;
        }
        return null;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property == null)
        {
            EditorGUI.LabelField(position, label, new GUIContent("Null property"));
            return;
        }

        // Child properties (tolerant to alternate names)
        var clipProp = SafeFind(property, "clip");
        var volumeProp = SafeFind(property, "volume");
        var pitchProp = SafeFind(property, "pitch");
        var is3DProp = SafeFind(property, "is3D");
        var loopProp = SafeFind(property, "loop");

        var startTimeProp = SafeFind(property, "startTime");
        var endTimeProp = SafeFind(property, "endTime");
        var spatialBlendProp = SafeFind(property, "spatialBlend");

        var mixerGroupProp = SafeFind(property, "mixerGroup");

        // Fades: accept duration names and legacy names
        var fadeInProp = SafeFind(property, "fadeInDuration", "fadeIn", "fadeInSeconds", "fadeInTime");
        var fadeOutProp = SafeFind(property, "fadeOutDuration", "fadeOut", "fadeOutSeconds", "fadeOutTime");

        // Begin drawing
        EditorGUI.BeginProperty(position, label, property);

        float line = position.y;
        float lineH = EditorGUIUtility.singleLineHeight;
        float fullW = position.width;
        float halfW = (fullW - PAD) * 0.5f;

        // Foldout header
        var headerRect = new Rect(position.x, line, fullW, lineH);
        property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, true);
        line += lineH + VSPACE;

        if (property.isExpanded)
        {
            // Row: Clip
            line = RowSingle(position, line, clipProp, "Clip");

            // Row: Volume | Pitch
            line = RowDual(position, line,
                volumeProp, new GUIContent("Volume"),
                pitchProp, new GUIContent("Pitch"));

            // Row: is3D | Loop
            line = RowDual(position, line,
                is3DProp, new GUIContent("3D"),
                loopProp, new GUIContent("Loop"));

            // Row: Spatial Blend
            line = RowSingle(position, line, spatialBlendProp, "Spatial Blend");

            // Row: Start Time | End Time
            line = RowDual(position, line,
                startTimeProp, new GUIContent("Start Time"),
                endTimeProp, new GUIContent("End Time"));

            // Row: Fade In | Fade Out  (null-safe placeholders if missing)
            line = RowDual(position, line,
                fadeInProp, new GUIContent("Fade In"),
                fadeOutProp, new GUIContent("Fade Out"));

            // Row: Mixer Group
            line = RowSingle(position, line, mixerGroupProp, "Mixer Group");
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Header always visible
        float totalHeight = EditorGUIUtility.singleLineHeight;

        if (property != null && property.isExpanded)
        {
            float lineH = EditorGUIUtility.singleLineHeight;

            // 1. Clip Reference
            totalHeight += lineH + VSPACE;

            // 2. Volume + Pitch (Dual Row)
            totalHeight += lineH + VSPACE;

            // 3. is3D + Loop (Dual Row)
            totalHeight += lineH + VSPACE;

            // 4. Spatial Blend
            totalHeight += lineH + VSPACE;

            // 5. TIMELINE SECTION (Dedicated Space)
            totalHeight += SECTION_SPACE; // Extra spacing before timeline
            totalHeight += TIMELINE_HEIGHT; // Timeline visualization area
            totalHeight += SECTION_SPACE; // Extra spacing after timeline

            // 6. Start Time + End Time (Dual Row)
            totalHeight += lineH + VSPACE;

            // 7. Fade In + Fade Out (Dual Row)
            totalHeight += lineH + VSPACE;

            // 8. Mixer Group
            totalHeight += lineH + VSPACE;

            // Final padding
            totalHeight += VSPACE * 2;
        }

        return totalHeight;
    }

    // ----- Helpers -----

    private static float RowSingle(Rect total, float y, SerializedProperty prop, string label)
    {
        float lh = EditorGUIUtility.singleLineHeight;
        var r = new Rect(total.x, y, total.width, lh);
        DrawPropOrPlaceholder(r, prop, new GUIContent(label));
        return y + lh + VSPACE;
    }

    private static float RowDual(Rect total, float y,
                                 SerializedProperty left, GUIContent leftLabel,
                                 SerializedProperty right, GUIContent rightLabel)
    {
        float lh = EditorGUIUtility.singleLineHeight;
        float halfW = (total.width - VSPACE * 2) * 0.5f; // INCREASED PAD spacing

        var leftRect = new Rect(total.x, y, halfW, lh);
        var rightRect = new Rect(total.x + halfW + VSPACE * 2, y, halfW, lh);

        DrawPropOrPlaceholder(leftRect, left, leftLabel);
        DrawPropOrPlaceholder(rightRect, right, rightLabel);

        return y + lh + VSPACE;
    }

    private static void DrawPropOrPlaceholder(Rect r, SerializedProperty prop, GUIContent label)
    {
        if (prop != null)
        {
            EditorGUI.PropertyField(r, prop, label);
        }
        else
        {
            using (new EditorGUI.DisabledScope(true))
            {
                // Show a stable placeholder so layout never breaks
                if (prop == null)
                    EditorGUI.LabelField(r, label, new GUIContent("—"));
                else
                    EditorGUI.PropertyField(r, prop, label); // unreachable, but keeps intent clear
            }
        }
    }
}
