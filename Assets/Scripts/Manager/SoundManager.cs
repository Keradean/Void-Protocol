/*
====================================================================
* SoundManager.cs - Audio Pool Priority System v3.3.2 (Phase 2 Complete)
====================================================================
* Project: Space Colony Game
* Script-Developer: Julian Gomez
* Created: 2025-09-15
* Last Modified: 2025-09-28
* Version: v3.3.2 - Phase 2 Walk Audio Integration Complete
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
* AUTHORSHIP CLASSIFICATION:
* [HUMAN-AUTHORED] - Audio system concept, walk audio integration concept
* [AI-ASSISTED] - Phase 2 implementation, debug parameter integration
* 
* PHASE 2 NOTES:
* - StartFootsteps method enhanced with reduced cadence testing
* - Walk audio volume doubled for immediate validation
* - Debug logging improved for walk audio troubleshooting
====================================================================
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public struct AudioClipData
{
    [Header("Audio Configuration")]
    public AudioClip clip;
    [Range(0f, 1f)] public float volume;
    [Range(0.5f, 2f)] public float pitch;
    public bool is3D;
    public bool loop;

    [Header("Timeline Control - Managed by Custom Editor")]
    [SerializeField] private float startTime;
    [SerializeField] private float endTime;
    [Range(0f, 1f)] public float spatialBlend;

    [Header("Advanced")]
    public AudioMixerGroup mixerGroup;

    [Header("Fades")]
    [Tooltip("Seconds to ramp in from 0 ? target volume")]
    [SerializeField, Range(0f, 3f)] public float fadeIn;

    [Tooltip("Seconds to ramp out to 0 at end")]
    [SerializeField, Range(0f, 3f)] public float fadeOut;

    public float StartTime
    {
        get => startTime;
        set => startTime = Mathf.Clamp(value, 0f, clip != null ? clip.length : 0f);
    }

    public float EndTime
    {
        get => endTime <= 0f ? (clip != null ? clip.length : 0f) : endTime;
        set => endTime = Mathf.Clamp(value, 0f, clip != null ? clip.length : 0f);
    }

    public float ClipDuration => Mathf.Max(0f, EndTime - StartTime);
}

[System.Serializable]
public struct GameDialogue
{
    public string dialogueID;
    public AudioClipData clip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    // === AUDIO DEBUG HELPERS ===
    public static bool AUDIO_DEBUG = true;
    private static int __audioSeq = 0;

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private static void DLog(string message)
    {
        if (!AUDIO_DEBUG) return;
        __audioSeq++;
        Debug.Log($"[AUDIO #{__audioSeq}] {message}");
    }

    // === SERIALIZED AUDIO SETS ===

    [Header("Movement Audio")]
    [SerializeField] private AudioClipData walkSound;
    [SerializeField] private AudioClipData runSound;
    [SerializeField] private AudioClipData jumpSound;
    [SerializeField] private AudioClipData landingSound;

    [Header("Combat Audio")]
    [SerializeField] private AudioClipData weaponShot;
    [SerializeField] private AudioClipData weaponEmpty;
    [SerializeField] private AudioClipData weaponReload;
    [SerializeField] private AudioClipData ImpactObject;
    [SerializeField] private AudioClipData ImpactMob;
    [SerializeField] private AudioClipData SpiderAttack;
    [SerializeField] private AudioClipData SpiderDefeat;
    [SerializeField] private AudioClipData SpiderMovement;

    [Header("Interaction Audio")]
    [SerializeField] private AudioClipData pickupItem;
    [SerializeField] private AudioClipData mediPenUse;
    [SerializeField] private AudioClipData doorOpensClose;
    [SerializeField] private AudioClipData terminalActivation;
    [SerializeField] private AudioClipData countdownTick;
    [SerializeField] private AudioClipData zoneCompletion;
    [SerializeField] private AudioClipData finalCompletionSignal;

    [Header("Hovercraft Category")]
    [SerializeField] private AudioClipData landingSequence;
    [SerializeField] private AudioClipData extractionSequence;

    [Header("Dialogue Audio")]
    [SerializeField] private GameDialogue[] gameDialogues;

    [Header("Ambient Music")]
    [SerializeField] private AudioClipData[] ambientMusicTracks;

    [Header("Game Music")]
    [SerializeField] private AudioClipData gameStartMusic;
    [SerializeField] private AudioClipData creditsMusic;
    [SerializeField] private AudioClipData extractionMusic;

    [Header("SFX")]
    [SerializeField] private AudioClipData uiClick;
    [SerializeField] private AudioClipData uiBack;


    [Header("Audio Mixer Groups")]
    [SerializeField] private AudioMixerGroup masterMixer;
    [SerializeField] private AudioMixerGroup musicMixer;
    [SerializeField] private AudioMixerGroup sfxMixer;
    [SerializeField] private AudioMixerGroup voiceMixer;
    [SerializeField] private AudioMixerGroup ambientMixer;

    [Header("Pooling Settings")]
    [Tooltip("Anzahl der vorgehaltenen Pooled AudioSources")]
    [SerializeField, Range(8, 64)] private int poolSize = 26;
    [Tooltip("Maximale gleichzeitige OneShots bevor lower-priority recycelt werden")]
    [SerializeField, Range(8, 64)] private int maxConcurrentSounds = 24;
    [Tooltip("Pooling aktivieren (empfohlen)")]
    [SerializeField] private bool enableObjectPooling = true;

    [Header("Footstep Controls")]
    [Tooltip("Minimales Zeitintervall zwischen zwei WALK-Schritten (Sekunden)")]
    [SerializeField, Range(0.05f, 1.0f)] private float walkMinInterval = 0.35f;

    [Tooltip("Maximale Abspieldauer pro Footstep-OneShot, um Pool-Blockaden zu vermeiden (Sekunden)")]
    [SerializeField, Range(0.1f, 2.0f)] private float footstepMaxDuration = 0.5f;

    [Tooltip("Kurze Gnadenzeit beim Stop, statt hartem Abbruch (Sekunden)")]
    [SerializeField, Range(0.05f, 0.35f)] private float walkStopGrace = 0.12f;

    // Audio Sources - Internal Management Only
    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource voiceSource;
    private AudioSource ambientSource;
    private AudioSource continuousFootstepSource;
    private AudioSource dialogueSource;

    // Audio pooling and state management
    private readonly Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private readonly List<AudioSource> activeWalkSources = new List<AudioSource>();

    // STATE
    private bool isFootstepPlaying = false;
    private bool isInitialized = false;

    // WALK cadence timer

    // === WALK DEBOUNCING SUPPORT ===
    private int lastWalkFrame = -1;
    private float walkCooldownUntil = 0f;

    // RUN state + sticky window to suppress brief flicker overlap
    private bool isRunActive = false;
    private float runStickyUntil = 0f;

    // PRIORITY SYSTEM
    public enum AudioPriority
    {
        Combat = 64,
        Movement = 48,
        Interaction = 32,
        Ambient = 16,
        Music = 8
    }



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAudioSources();
        InitializePool();

        isInitialized = true;

        if (ambientMusicTracks != null && ambientMusicTracks.Length > 0)
        {
            DLog("AmbientMusicTracks detected -> PlayAmbientMusic()");
            PlayAmbientMusic();
        }
        else
        {
            DLog("No AmbientMusicTracks configured");
        }

        // ? DIALOGUE SOURCE INITIALIZATION
        GameObject dialogueGO = new GameObject("DialogueSource");
        dialogueGO.transform.SetParent(transform);
        dialogueSource = dialogueGO.AddComponent<AudioSource>();
        dialogueSource.outputAudioMixerGroup = voiceMixer;
        dialogueSource.spatialBlend = 0f; // 2D for dialogue
        dialogueSource.playOnAwake = false;
        dialogueSource.priority = 10; // High priority for dialogue

        DLog("Dialogue Source initialized successfully");
    }


    public void PlayDialogue(AudioClip clip)
    {
        if (clip == null || dialogueSource == null) return;

        dialogueSource.Stop();
        dialogueSource.clip = clip;
        dialogueSource.Play();
    }

    private void InitializeAudioSources()
    {
        musicSource = CreateAudioSource("MusicSource", 0.7f, true);
        musicSource.outputAudioMixerGroup = musicMixer;

        sfxSource = CreateAudioSource("SFXSource", 1.0f, false);
        sfxSource.outputAudioMixerGroup = sfxMixer;

        voiceSource = CreateAudioSource("VoiceSource", 1.0f, false);
        voiceSource.outputAudioMixerGroup = voiceMixer;

        ambientSource = CreateAudioSource("AmbientSource", 0.8f, true);
        ambientSource.outputAudioMixerGroup = ambientMixer;

        continuousFootstepSource = CreateAudioSource("FootstepSource", 1.0f, true);
        continuousFootstepSource.outputAudioMixerGroup = sfxMixer;
        continuousFootstepSource.loop = true;
        continuousFootstepSource.spatialBlend = 1.0f; // 3D
    }

    private void InitializePool()
    {
        if (!enableObjectPooling) return;

        for (int i = 0; i < poolSize; i++)
        {
            var src = CreateAudioSource($"PooledAudioSource_{i}", 1.0f, false);
            src.outputAudioMixerGroup = sfxMixer;
            src.spatialBlend = 1.0f;
            audioSourcePool.Enqueue(src);
        }
    }

    private AudioSource CreateAudioSource(string name, float volume, bool playOnAwake)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = playOnAwake;
        src.volume = volume;
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        src.priority = 100;
        return src;
    }

    // ===== MOVEMENT AUDIO - ENHANCED WITH PHASE 2 =====
    public void StartFootsteps(bool isRunning, Vector3 position)
    {
        DLog($"StartFootsteps isRunning={isRunning} pos={position} init={isInitialized}");
        if (!isInitialized) { DLog("DROP StartFootsteps: init=false"); return; }

        if (isRunning)
        {
            // Run: Continuous loop + FOLLOW position
            if (runSound.clip != null && !continuousFootstepSource.isPlaying)
            {
                continuousFootstepSource.clip = runSound.clip;
                continuousFootstepSource.volume = runSound.volume;
                continuousFootstepSource.pitch = runSound.pitch;
                continuousFootstepSource.time = runSound.StartTime;
                continuousFootstepSource.loop = true;
                continuousFootstepSource.transform.position = position;

                // ensure segment loop eligibility
                runSound.loop = true; // runtime flag for segment rewind

                DLog($"Footstep RUN start clip={continuousFootstepSource.clip.name} vol={continuousFootstepSource.volume:F2} pitch={continuousFootstepSource.pitch:F2}");

                continuousFootstepSource.Play();

                // mark RUN active and start a small sticky window
                isRunActive = true;
                runStickyUntil = Time.time + 0.15f; // 150 ms stickiness

                if (runSound.ClipDuration < runSound.clip.length)
                {
                    DLog($"Footstep RUN segmented looping dur={runSound.ClipDuration:F2}s from={runSound.StartTime:F2}s");
                    StartCoroutine(LoopSegment(continuousFootstepSource, runSound));
                }
                isFootstepPlaying = true;
            }
            else if (runSound.clip == null)
            {
                DLog("RUN requested but runSound.clip == null");
            }
            else
            {
                // already playing -> update position to follow the player
                continuousFootstepSource.transform.position = position;

                // keep RUN marked active and extend sticky window while running
                isRunActive = true;
                runStickyUntil = Time.time + 0.15f;
            }
        }
        else
        {
            // WALK: OneShot with REDUCED CADENCE for testing + INCREASED VOLUME
            // ---- WALK (OneShot, debounced) ----
            if (walkSound.clip != null)
            {
                // DIAG + GUARD: If RUN is active or within sticky window, suppress WALK
                if ((continuousFootstepSource != null && continuousFootstepSource.isPlaying) || isRunActive)
                {
                    if (Time.time < runStickyUntil)
                    {
                        DLog("DIAG: WALK suppressed because RUN is active/sticky -> prevents overlap");
                        return;
                    }
                }

                // Frame gate: block multiple same-frame calls
                if (Time.frameCount == lastWalkFrame) return;

                // Cadence gate
                if (Time.time < walkCooldownUntil)
                {
                    DLog($"Footstep WALK throttled - wait {walkCooldownUntil - Time.time:F2}s");
                    return;
                }

                // Use Inspector’s walkMinInterval, not temporary test value
                walkCooldownUntil = Time.time + walkMinInterval;
                lastWalkFrame = Time.frameCount;

                AudioSource walkSource = GetPooledAudioSource(AudioPriority.Movement);
                if (walkSource != null)
                {
                    walkSource.clip = walkSound.clip;
                    walkSource.volume = walkSound.volume;
                    walkSource.pitch = walkSound.pitch;
                    walkSource.time = walkSound.StartTime;
                    walkSource.spatialBlend = walkSound.is3D ? 1f : 0f;
                    walkSource.transform.position = position;
                    DLog($"Footstep WALK play clip={walkSource.clip.name} vol={walkSource.volume:F2} pitch={walkSource.pitch:F2} id={walkSource.GetInstanceID()}");
                    walkSource.Play();

                    float duration = walkSound.ClipDuration;
                    if (duration <= 0.01f || duration > footstepMaxDuration)
                        duration = Mathf.Clamp(duration, 0.1f, footstepMaxDuration);

                    activeWalkSources.Add(walkSource);
                    ScheduleReturn(walkSource, duration);
                }
                else
                {
                    DLog("Footstep WALK request -> POOL EMPTY (no source)");
                }
            }
            else
            {
                DLog("WALK requested but walkSound.clip == null");
            }
        }
    }

    public void StopFootsteps()
    {
        DLog("StopFootsteps()");
        // Nur RUN-Loop stoppen. WALK-OneShots in Ruhe lassen.
        if (continuousFootstepSource != null && isFootstepPlaying)
        {
            DLog($"Footstep RUN stop wasPlaying={continuousFootstepSource.isPlaying}");
            continuousFootstepSource.Stop();
            isFootstepPlaying = false;

            // clear RUN state and start brief grace period for WALK
            isRunActive = false;
            runStickyUntil = 0f;
            walkCooldownUntil = Mathf.Max(walkCooldownUntil, Time.time + walkStopGrace);
            DLog($"Run stopped -> WALK grace {walkStopGrace:F2}s");
        }
    }

    public void PlayJump(Vector3 position)
    {
        DLog($"PlayJump pos={position} init={isInitialized}");
        if (!isInitialized) { DLog("DROP PlayJump: init=false"); return; }
        if (jumpSound.clip == null) { DLog("DROP PlayJump: clip=null"); return; }
        PlaySound3D(jumpSound, position, AudioPriority.Movement);
    }

    public void PlayLanding(Vector3 position)
    {
        DLog($"PlayLanding pos={position} init={isInitialized}");
        if (!isInitialized) { DLog("DROP PlayLanding: init=false"); return; }
        if (landingSound.clip == null) { DLog("DROP PlayLanding: clip=null"); return; }

        AudioSource landingSource = GetPooledAudioSource(AudioPriority.Movement);
        if (landingSource != null)
        {
            landingSource.clip = landingSound.clip;
            landingSource.volume = landingSound.volume;
            landingSource.pitch = landingSound.pitch;
            landingSource.time = landingSound.StartTime;
            landingSource.spatialBlend = landingSound.is3D ? 1f : 0f;
            landingSource.transform.position = position;
            DLog($"Landing play clip={landingSource.clip.name} vol={landingSource.volume:F2} pitch={landingSource.pitch:F2}");
            landingSource.Play();

            float duration = Mathf.Max(0.05f, landingSound.ClipDuration);
            ScheduleReturn(landingSource, duration);
        }
        else
        {
            DLog("Landing request -> POOL EMPTY (no source)");
        }
    }

    // ===== INTERACTION AUDIO =====
    public void PlayPickupItem(Vector3 position)
    {
        if (!isInitialized) { DLog("DROP PlayPickupItem: init=false"); return; }
        if (pickupItem.clip == null) { DLog("DROP PlayPickupItem: clip=null"); return; }
        PlaySound3D(pickupItem, position, AudioPriority.Interaction);
    }

    // ===== ZONE & MISSION AUDIO =====
    public void PlayZoneCompletion(Vector3 position)
    {
        if (!isInitialized) { DLog("DROP PlayZoneCompletion: init=false"); return; }
        if (zoneCompletion.clip == null) { DLog("DROP PlayZoneCompletion: clip=null"); return; }
        PlaySound3D(zoneCompletion, position, AudioPriority.Interaction);
    }

    public void PlayFinalCompletion(Vector3 position)
    {
        if (!isInitialized) { DLog("DROP PlayFinalCompletion: init=false"); return; }
        if (finalCompletionSignal.clip == null) { DLog("DROP PlayFinalCompletion: clip=null"); return; }
        PlaySound3D(finalCompletionSignal, position, AudioPriority.Interaction);
    }

    public void PlayTerminalActivation(Vector3 position)
    {
        if (!isInitialized) { DLog("DROP PlayTerminalActivation: init=false"); return; }
        if (terminalActivation.clip == null) { DLog("DROP PlayTerminalActivation: clip=null"); return; }
        PlaySound3D(terminalActivation, position, AudioPriority.Interaction);
    }

    // ===== COMBAT SUPPORT AUDIO =====
    public void PlayImpactSoundMobs(Vector3 position)
    {
        if (!isInitialized) { DLog("DROP PlayImpactSoundMobs: init=false"); return; }
        if (weaponShot.clip == null) { DLog("DROP PlayImpactSoundMobs: clip=null"); return; }
        PlaySound3D(weaponShot, position, AudioPriority.Combat);
    }

    public void PlayImpactSoundObjects(Vector3 position)
    {
        if (!isInitialized) { DLog("DROP PlayImpactSoundObjects: init=false"); return; }
        if (weaponShot.clip == null) { DLog("DROP PlayImpactSoundObjects: clip=null"); return; }
        PlaySound3D(weaponShot, position, AudioPriority.Combat);
    }

    public void PlayWeaponEmpty(Vector3 position)
    {
        if (!isInitialized) { DLog("DROP PlayWeaponEmpty: init=false"); return; }
        if (weaponEmpty.clip == null) { DLog("DROP PlayWeaponEmpty: clip=null"); return; }
        PlaySound3D(weaponEmpty, position, AudioPriority.Combat);
    }

    public void PlayReloadSound(Vector3 position)
    {
        if (!isInitialized) { DLog("DROP PlayReloadSound: init=false"); return; }
        if (weaponReload.clip == null) { DLog("DROP PlayReloadSound: clip=null"); return; }
        PlaySound3D(weaponReload, position, AudioPriority.Combat);
    }

    // ===== ENEMY AUDIO - Added by Julian with AI-Support =====
    public void PlaySpiderAttack(Vector3 position)
    {
        if (!isInitialized) { DLog("DROP PlaySpiderAttack: init=false"); return; }
        if (SpiderAttack.clip == null) { DLog("DROP PlaySpiderAttack: clip=null"); return; }
        PlaySound3D(SpiderAttack, position, AudioPriority.Combat);
    }

    public void PlaySpiderDefeat(Vector3 position)
    {
        if (!isInitialized) { DLog("DROP PlaySpiderDefeat: init=false"); return; }
        if (SpiderDefeat.clip == null) { DLog("DROP PlaySpiderDefeat: clip=null"); return; }
        PlaySound3D(SpiderDefeat, position, AudioPriority.Combat);
    }

    public void PlaySpiderMovement(Vector3 position)
    {
        if (!isInitialized) { DLog("DROP PlaySpiderMovement: init=false"); return; }
        if (SpiderMovement.clip == null) { DLog("DROP PlaySpiderMovement: clip=null"); return; }
        PlaySound3D(SpiderMovement, position, AudioPriority.Movement);
    }

    // ===== COMBAT AUDIO =====
    public void PlayWeaponSound(int weaponIndex, Vector3 position)
    {
        if (!isInitialized) { DLog("DROP PlayWeaponSound: init=false"); return; }
        if (weaponShot.clip == null) { DLog("DROP PlayWeaponSound: clip=null"); return; }

        AudioSource src = GetPooledAudioSource(AudioPriority.Combat);
        if (src != null)
        {
            src.clip = weaponShot.clip;
            src.volume = weaponShot.volume;
            src.pitch = weaponShot.pitch;
            src.time = weaponShot.StartTime;
            src.spatialBlend = weaponShot.is3D ? 1f : 0f;
            src.transform.position = position;
            src.Play();

            float duration = Mathf.Max(0.05f, weaponShot.ClipDuration);
            ScheduleReturn(src, duration);
        }
    }

    // ===== CORE PLAY HELPERS =====
    private void PlaySound2D(AudioClipData data)
    {
        if (data.clip == null) return;
        sfxSource.outputAudioMixerGroup = sfxMixer;
        sfxSource.clip = data.clip;
        sfxSource.volume = data.volume;
        sfxSource.pitch = data.pitch;
        sfxSource.time = data.StartTime;
        sfxSource.loop = data.loop;
        sfxSource.Play();
    }

    private void PlaySound3D(AudioClipData data, Vector3 position, AudioPriority priority)
    {
        if (data.clip == null) return;
        AudioSource src = GetPooledAudioSource(priority);
        if (src == null) return;

        src.clip = data.clip;
        src.volume = data.volume;
        src.pitch = data.pitch;
        src.time = data.StartTime;
        src.spatialBlend = data.is3D ? 1f : 0f;
        src.transform.position = position;
        src.loop = data.loop;
        src.Play();

        float duration = data.ClipDuration;
        if (!data.loop)
        {
            if (duration <= 0.01f || duration > 6.0f)
                duration = Mathf.Clamp(duration, 0.05f, 6.0f);
            ScheduleReturn(src, duration);
        }
    }

    private void ScheduleReturn(AudioSource src, float delay)
    {
        StartCoroutine(ReturnAfter(src, delay));
    }

    private IEnumerator ReturnAfter(AudioSource src, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (src == null) yield break;
        src.Stop();
        src.clip = null;
        activeWalkSources.Remove(src);
        audioSourcePool.Enqueue(src);
    }

    private AudioSource GetPooledAudioSource(AudioPriority priority)
    {
        if (!enableObjectPooling)
        {
            var go = new GameObject("TempAudio");
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxMixer;
            src.spatialBlend = 1f;
            return src;
        }

        if (audioSourcePool.Count > 0)
        {
            var src = audioSourcePool.Dequeue();
            return src;
        }

        // Recycle if over limit
        if (activeWalkSources.Count >= maxConcurrentSounds)
        {
            var src = activeWalkSources[0];
            activeWalkSources.RemoveAt(0);
            return src;
        }

        // As a fallback, create a new one (rare)
        var extra = CreateAudioSource($"PooledAudioSource_{Random.Range(1000, 9999)}", 1.0f, false);
        extra.outputAudioMixerGroup = sfxMixer;
        extra.spatialBlend = 1f;
        return extra;
    }

    private IEnumerator LoopSegment(AudioSource source, AudioClipData data)
    {
        // Loop a segment [StartTime, EndTime]
        while (source != null && source.isPlaying && data.loop)
        {
            if (source.time >= data.EndTime)
            {
                source.time = data.StartTime;
            }
            yield return null;
        }
    }

    // ===== AMBIENT / MUSIC =====
    private void PlayAmbientMusic()
    {
        if (ambientMusicTracks == null || ambientMusicTracks.Length == 0) return;

        var track = ambientMusicTracks[Random.Range(0, ambientMusicTracks.Length)];
        musicSource.outputAudioMixerGroup = musicMixer;
        musicSource.clip = track.clip;
        musicSource.volume = track.volume;
        musicSource.pitch = track.pitch;
        musicSource.time = track.StartTime;
        musicSource.loop = true;
        musicSource.Play();
    }
}
