using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Robust Editor-only audio preview for Unity 6.
/// - Toggle segment playback from start..end
/// - Works across AudioUtil overload variations
/// - Auto-stops at end (when position APIs exist)
/// </summary>
public static class AudioPreviewUtility
{
    private static readonly Type T_AudioUtil = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");

    private static MethodInfo M(string name, params Type[] args)
    {
        if (T_AudioUtil == null) return null;
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        return T_AudioUtil.GetMethod(name, flags, null, args, null);
    }

    // Try common signatures seen across versions
    private static readonly MethodInfo MI_PlayClip_3 = M("PlayClip", typeof(AudioClip), typeof(int), typeof(bool));
    private static readonly MethodInfo MI_PlayClip_1 = M("PlayClip", typeof(AudioClip));
    private static readonly MethodInfo MI_PlayPreviewClip_3 = M("PlayPreviewClip", typeof(AudioClip), typeof(int), typeof(bool));

    private static readonly MethodInfo MI_StopAllClips = M("StopAllClips");
    private static readonly MethodInfo MI_StopClip = M("StopClip", typeof(AudioClip));
    private static readonly MethodInfo MI_StopPreviewClip = M("StopPreviewClip", typeof(AudioClip));

    private static readonly MethodInfo MI_IsClipPlaying = M("IsClipPlaying", typeof(AudioClip));
    private static readonly MethodInfo MI_GetClipPosition = M("GetClipPosition", typeof(AudioClip));
    private static readonly MethodInfo MI_SetClipSamplePos = M("SetClipSamplePosition", typeof(AudioClip), typeof(int));

    private static AudioClip activeClip;
    private static double endTimeSec = -1;
    private static bool updateHooked;

    // Public API ---------------------------------------------------------------

    public static bool IsPlaying(AudioClip clip)
    {
        if (clip == null) return false;

        if (MI_IsClipPlaying != null)
        {
            try { return (bool)MI_IsClipPlaying.Invoke(null, new object[] { clip }); }
            catch { /* fallthrough */ }
        }

        // Fallback heuristic: if our activeClip equals the clip and we have an end scheduled,
        // assume playing (not perfect, but better than nothing).
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
        try
        {
            if (clip != null)
            {
                if (MI_StopClip != null) MI_StopClip.Invoke(null, new object[] { clip });
                if (MI_StopPreviewClip != null) MI_StopPreviewClip.Invoke(null, new object[] { clip });
            }

            if (MI_StopAllClips != null) MI_StopAllClips.Invoke(null, null);

            // Unity 6: try StopAllPreviewClips (newer internal API)
            var miStopAllPreview = T_AudioUtil.GetMethod(
                "StopAllPreviewClips",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
            );
            if (miStopAllPreview != null) miStopAllPreview.Invoke(null, null);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("AudioPreviewUtility.Stop exception: " + ex.Message);
        }

        ClearEndHook();
    }


    // Internals ----------------------------------------------------------------

    private static void PlaySegment(AudioClip clip, float startSec, float endSec)
    {
        if (clip == null) return;

        startSec = Mathf.Clamp(startSec, 0f, clip.length);
        endSec = Mathf.Clamp(endSec, startSec, clip.length);
        int startSample = Mathf.RoundToInt(startSec * clip.frequency);

        // Make sure nothing else is playing
        Stop(null);

        // Preferred: PlayClip(clip, startSample, loop=false)
        if (InvokeSafe(MI_PlayClip_3, clip, startSample, false)) { SetupEndHook(clip, endSec); return; }

        // Next: PlayPreviewClip(clip, startSample, loop=false)
        if (InvokeSafe(MI_PlayPreviewClip_3, clip, startSample, false)) { SetupEndHook(clip, endSec); return; }

        // Fallback: SetClipSamplePosition + PlayClip(clip)
        if (InvokeSafe(MI_SetClipSamplePos, clip, startSample) && InvokeSafe(MI_PlayClip_1, clip))
        {
            SetupEndHook(clip, endSec); return;
        }

        // Last resort: PlayClip(clip) (no start position)
        if (InvokeSafe(MI_PlayClip_1, clip)) { SetupEndHook(clip, endSec); return; }

        Debug.LogWarning("AudioPreviewUtility: No usable play path found on AudioUtil.");
    }

    private static bool InvokeSafe(MethodInfo mi, params object[] args)
    {
        if (mi == null) return false;
        try { mi.Invoke(null, args); return true; } catch { return false; }
    }

    private static void SetupEndHook(AudioClip clip, double endSec)
    {
        activeClip = clip;
        endTimeSec = endSec;

        if (MI_IsClipPlaying != null && MI_GetClipPosition != null)
        {
            HookUpdate();
        }
        else
        {
            // If position polling is unavailable, we cannot auto-stop.
            // The toggle will still stop on next click.
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
        EditorApplication.update += Update;
        updateHooked = true;
    }

    private static void UnhookUpdate()
    {
        if (!updateHooked) return;
        EditorApplication.update -= Update;
        updateHooked = false;
    }

    private static void Update()
    {
        if (activeClip == null || MI_IsClipPlaying == null || MI_GetClipPosition == null)
        {
            UnhookUpdate(); return;
        }

        bool playing;
        double pos;

        try { playing = (bool)MI_IsClipPlaying.Invoke(null, new object[] { activeClip }); }
        catch { UnhookUpdate(); return; }

        if (!playing) { ClearEndHook(); return; }

        try { pos = Convert.ToDouble(MI_GetClipPosition.Invoke(null, new object[] { activeClip })); }
        catch { UnhookUpdate(); return; }

        if (endTimeSec >= 0 && pos >= endTimeSec)
        {
            Stop(activeClip);
        }
    }
}
