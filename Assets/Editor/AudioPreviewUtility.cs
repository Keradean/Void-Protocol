/*
====================================================================
* AudioPreviewUtility.cs - Editor Audio Preview System v3.0
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
* [HUMAN-AUTHORED] - Segment playback concept, Unity AudioUtil integration approach
* [AI-ASSISTED] - Reflection optimization, conflict resolution, robust error handling
* 
* BEREINIGUNGSNOTIZEN v3.0:
* - Performance-Optimierung: Cached Reflection-Calls für AudioUtil Methods
* - Error-Handling: Robuste Fallback-Mechanismen ohne redundante Try-Catch
* - Code-Struktur: Eliminierung doppelter Method-Lookups und optimierte State-Management
====================================================================
*/

#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Robust Editor-only audio preview for Unity 6 with optimized reflection and conflict-free implementation
/// - Toggle segment playback from start..end
/// - Works across AudioUtil overload variations
/// - Auto-stops at end (when position APIs exist)
/// - Performance optimized with cached reflection calls
/// </summary>
public static class AudioPreviewUtility
{
    // Cached reflection data - performance optimization
    private static readonly Type T_AudioUtil = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
    private static readonly object reflectionLock = new object();

    // Cached method references - conflict-free implementation
    private static class CachedMethods
    {
        public static readonly MethodInfo PlayClip_3 = GetMethod("PlayClip", typeof(AudioClip), typeof(int), typeof(bool));
        public static readonly MethodInfo PlayClip_1 = GetMethod("PlayClip", typeof(AudioClip));
        public static readonly MethodInfo PlayPreviewClip_3 = GetMethod("PlayPreviewClip", typeof(AudioClip), typeof(int), typeof(bool));

        public static readonly MethodInfo StopAllClips = GetMethod("StopAllClips");
        public static readonly MethodInfo StopClip = GetMethod("StopClip", typeof(AudioClip));
        public static readonly MethodInfo StopPreviewClip = GetMethod("StopPreviewClip", typeof(AudioClip));
        public static readonly MethodInfo StopAllPreviewClips = GetMethod("StopAllPreviewClips");

        public static readonly MethodInfo IsClipPlaying = GetMethod("IsClipPlaying", typeof(AudioClip));
        public static readonly MethodInfo GetClipPosition = GetMethod("GetClipPosition", typeof(AudioClip));
        public static readonly MethodInfo SetClipSamplePos = GetMethod("SetClipSamplePosition", typeof(AudioClip), typeof(int));

        private static MethodInfo GetMethod(string name, params Type[] args)
        {
            if (T_AudioUtil == null) return null;

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            return T_AudioUtil.GetMethod(name, flags, null, args, null);
        }
    }

    // State management - conflict-free implementation
    private static AudioClip activeClip;
    private static double endTimeSec = -1;
    private static bool updateHooked;

    // Public API - optimized and conflict-free

    public static bool IsPlaying(AudioClip clip)
    {
        if (clip == null) return false;

        // Primary method: Use reflection if available
        if (CachedMethods.IsClipPlaying != null)
        {
            return InvokeSafeBool(CachedMethods.IsClipPlaying, clip);
        }

        // Fallback: Use state tracking
        return activeClip == clip && endTimeSec >= 0;
    }

    public static void ToggleSegment(AudioClip clip, float startSec, float endSec)
    {
        if (clip == null) return;

        if (IsPlaying(clip))
        {
            Stop(clip);
        }
        else
        {
            PlaySegment(clip, startSec, endSec);
        }
    }

    public static void Stop(AudioClip clip)
    {
        lock (reflectionLock)
        {
            // Specific clip stop attempts
            if (clip != null)
            {
                InvokeSafe(CachedMethods.StopClip, clip);
                InvokeSafe(CachedMethods.StopPreviewClip, clip);
            }

            // Global stop attempts - conflict resolution
            InvokeSafe(CachedMethods.StopAllClips);
            InvokeSafe(CachedMethods.StopAllPreviewClips);

            ClearEndHook();
        }
    }

    // Internal implementation - optimized and conflict-free

    private static void PlaySegment(AudioClip clip, float startSec, float endSec)
    {
        if (clip == null) return;

        // Validate and clamp parameters
        startSec = Mathf.Clamp(startSec, 0f, clip.length);
        endSec = Mathf.Clamp(endSec, startSec, clip.length);
        int startSample = Mathf.RoundToInt(startSec * clip.frequency);

        // Ensure clean state
        Stop(null);

        // Try playback methods in priority order - conflict-free implementation
        bool playbackStarted = false;

        // Method 1: PlayClip(clip, startSample, loop=false)
        if (!playbackStarted && CachedMethods.PlayClip_3 != null)
        {
            playbackStarted = InvokeSafe(CachedMethods.PlayClip_3, clip, startSample, false);
        }

        // Method 2: PlayPreviewClip(clip, startSample, loop=false)
        if (!playbackStarted && CachedMethods.PlayPreviewClip_3 != null)
        {
            playbackStarted = InvokeSafe(CachedMethods.PlayPreviewClip_3, clip, startSample, false);
        }

        // Method 3: SetClipSamplePosition + PlayClip(clip)
        if (!playbackStarted && CachedMethods.SetClipSamplePos != null && CachedMethods.PlayClip_1 != null)
        {
            if (InvokeSafe(CachedMethods.SetClipSamplePos, clip, startSample))
            {
                playbackStarted = InvokeSafe(CachedMethods.PlayClip_1, clip);
            }
        }

        // Method 4: PlayClip(clip) fallback
        if (!playbackStarted && CachedMethods.PlayClip_1 != null)
        {
            playbackStarted = InvokeSafe(CachedMethods.PlayClip_1, clip);
        }

        if (playbackStarted)
        {
            SetupEndHook(clip, endSec);
        }
    }

    private static bool InvokeSafe(MethodInfo method, params object[] args)
    {
        if (method == null) return false;

        try
        {
            method.Invoke(null, args);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool InvokeSafeBool(MethodInfo method, params object[] args)
    {
        if (method == null) return false;

        try
        {
            var result = method.Invoke(null, args);
            return result is bool boolResult && boolResult;
        }
        catch
        {
            return false;
        }
    }

    private static double InvokeSafeDouble(MethodInfo method, params object[] args)
    {
        if (method == null) return 0.0;

        try
        {
            var result = method.Invoke(null, args);
            return Convert.ToDouble(result);
        }
        catch
        {
            return 0.0;
        }
    }

    // Hook management - conflict-free state management

    private static void SetupEndHook(AudioClip clip, double endSec)
    {
        activeClip = clip;
        endTimeSec = endSec;

        if (CachedMethods.IsClipPlaying != null && CachedMethods.GetClipPosition != null)
        {
            HookUpdate();
        }
        else
        {
            // Position polling unavailable - manual tracking only
            UnhookUpdate();
        }
    }

    private static void ClearEndHook()
    {
        activeClip = null;
        endTimeSec = -1;
        UnhookUpdate();
    }

    private static void HookUpdate()
    {
        if (updateHooked) return;
        EditorApplication.update += UpdateHook;
        updateHooked = true;
    }

    private static void UnhookUpdate()
    {
        if (!updateHooked) return;
        EditorApplication.update -= UpdateHook;
        updateHooked = false;
    }

    private static void UpdateHook()
    {
        // Validation - conflict-free implementation
        if (activeClip == null ||
            CachedMethods.IsClipPlaying == null ||
            CachedMethods.GetClipPosition == null)
        {
            UnhookUpdate();
            return;
        }

        // Check if still playing
        if (!InvokeSafeBool(CachedMethods.IsClipPlaying, activeClip))
        {
            ClearEndHook();
            return;
        }

        // Check position for auto-stop
        double currentPos = InvokeSafeDouble(CachedMethods.GetClipPosition, activeClip);

        if (endTimeSec >= 0 && currentPos >= endTimeSec)
        {
            Stop(activeClip);
        }
    }
}
#endif