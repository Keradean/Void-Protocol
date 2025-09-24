/*
====================================================================
* SoundManager.cs - Complete Audio System Enhanced
====================================================================
* Project: Space Colony Game
* Course: PIP
* Script-Developer: Julian
* Date: 23.09.2025
* Version: v1.3 - Enhanced Audio Control with Manual Timing
*
* WICHTIG: KOMMENTIERUNG NICHT LÖSCHEN!
* Diese detaillierte Authorship-Dokumentation ist für die
* akademische Bewertung erforderlich und darf nicht entfernt werden!
*
* AUTHORSHIP CLASSIFICATION:
* [HUMAN-AUTHORED] - Audio requirements, game integration concept
* [AI-ASSISTED] - Enhanced audio control system, responsive playback
====================================================================
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AudioClipData
{
    [Header("Audio Configuration")]
    public AudioClip clip;
    [Range(0f, 1f)] public float volume;
    [Range(0.5f, 2f)] public float pitch;
    public bool is3D;
    public bool loop;

    [Header("Manual Audio Control - Added by Julian with AI-Support")]
    [SerializeField] private float startTime;
    [SerializeField] private float endTime;
    [Range(0f, 1f)] public float fadeInDuration;
    [Range(0f, 1f)] public float fadeOutDuration;

    // Properties for manual audio timing control
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

    public float ClipDuration => EndTime - StartTime;
}

[CreateAssetMenu(fileName = "SoundConfiguration", menuName = "Space Colony/Sound Configuration")]
public class SoundConfiguration : ScriptableObject
{
    [Header("Movement Audio")]
    public AudioClipData[] walkSounds;
    public AudioClipData[] runSounds;
    public AudioClipData jumpSound;
    public AudioClipData landingSound;

    [Header("Interaction Audio")]
    public AudioClipData pickupAmmo;
    public AudioClipData pickupOxygen;
    public AudioClipData chipPlacement;
    public AudioClipData zoneComplete;

    [Header("Countdown System")]
    public AudioClipData countdownTick;
    public AudioClipData countdownMusic;
    public AudioClipData countdownFinal;

    [Header("Combat Audio")]
    public AudioClipData[] weapon1Sounds;
    public AudioClipData[] weapon2Sounds;
    public AudioClipData[] impactSounds;
    public AudioClipData weaponReload;
    public AudioClipData weaponEmpty;

    [Header("Mission Audio")]
    public AudioClipData hoverSound;
    public AudioClipData landingSequence;
    public AudioClipData extractionMusic;

    [Header("Dialogue Audio")]
    public AudioClipData missionStart;
    public AudioClipData missionConfirm;
    public AudioClipData zoneUpdate;
    public AudioClipData extractionCall;

    [Header("Music")]
    public AudioClipData gameStartMusic;
    public AudioClipData creditsMusic;
    public AudioClipData ambientMusic;

    public bool ValidateConfiguration()
    {
        return jumpSound.clip != null && pickupAmmo.clip != null && gameStartMusic.clip != null;
    }
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private SoundConfiguration soundConfig;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioSource ambientSource;

    [Header("3D Audio Settings")]
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    [Header("Performance")]
    [SerializeField] private int maxConcurrentSounds = 16;
    [SerializeField] private bool enableObjectPooling = true;

    [Header("Enhanced Audio Control - Added by Julian with AI-Support")]
    [SerializeField] private AudioSource continuousFootstepSource; // Dedicated source for continuous sounds
    private bool isFootstepPlaying = false;

    // Object Pooling
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private List<AudioSource> activeAudioSources = new List<AudioSource>();

    // System State
    private Coroutine countdownCoroutine;
    private bool isCountdownActive = false;
    private float lastFootstepTime;
    private float footstepInterval = 0.5f;
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeAudioSources();
        InitializeAudioPool();
        StartCoroutine(DelayedInitialization());
    }

    void Update()
    {
        // Performance monitoring if needed
    }

    private IEnumerator DelayedInitialization()
    {
        yield return new WaitForSeconds(0.1f);

        SubscribeToEvents();
        isInitialized = true;

        if (soundConfig != null && soundConfig.ambientMusic.clip != null)
        {
            PlayAmbientMusic();
        }
    }

    private void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.7f;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.volume = 0.8f;
        }

        if (voiceSource == null)
        {
            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.loop = false;
            voiceSource.volume = 0.9f;
        }

        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.volume = 0.4f;
        }

        if (continuousFootstepSource == null)
        {
            continuousFootstepSource = gameObject.AddComponent<AudioSource>();
            continuousFootstepSource.loop = true;
            continuousFootstepSource.volume = 0.6f;
            continuousFootstepSource.spatialBlend = 1f; // 3D audio for footsteps
        }
    }

    private void InitializeAudioPool()
    {
        if (!enableObjectPooling) return;

        for (int i = 0; i < maxConcurrentSounds; i++)
        {
            GameObject audioObj = new GameObject($"PooledAudioSource_{i}");
            audioObj.transform.SetParent(transform);

            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.rolloffMode = rolloffMode;
            source.maxDistance = maxDistance;

            audioSourcePool.Enqueue(source);
        }
    }

    private void SubscribeToEvents()
    {
        if (TileManager.Instance != null)
        {
            TileManager.Instance.OnZoneActivationStarted += PlayChipPlacement;
            TileManager.Instance.OnZoneActivationComplete += PlayZoneComplete;
        }
    }

    public void StartFootsteps(bool isRunning, Vector3 position)
    {
        if (!isInitialized || soundConfig == null) return;

        AudioClipData[] footstepSounds = isRunning ? soundConfig.runSounds : soundConfig.walkSounds;

        if (footstepSounds == null || footstepSounds.Length == 0) return;

        if (!isFootstepPlaying || (isRunning != (continuousFootstepSource.pitch > 1.2f)))
        {
            AudioClipData footstepData = footstepSounds[Random.Range(0, footstepSounds.Length)];

            if (footstepData.clip != null)
            {
                continuousFootstepSource.transform.position = position;
                continuousFootstepSource.clip = footstepData.clip;
                continuousFootstepSource.volume = footstepData.volume;
                continuousFootstepSource.pitch = isRunning ? 1.5f : 1.0f; // Faster pitch for running
                continuousFootstepSource.time = footstepData.StartTime;

                if (!isFootstepPlaying)
                {
                    continuousFootstepSource.Play();
                    isFootstepPlaying = true;
                }
            }
        }
        else
        {
            // Update position while moving
            continuousFootstepSource.transform.position = position;
        }
    }

    public void StopFootsteps()
    {
        if (isFootstepPlaying && continuousFootstepSource != null)
        {
            continuousFootstepSource.Stop();
            isFootstepPlaying = false;
        }
    }

    // LEGACY FOOTSTEP METHOD - kept for compatibility
    public void PlayFootstep(bool isRunning, Vector3 position)
    {
        StartFootsteps(isRunning, position);
    }

    public void PlayJump(Vector3 position)
    {
        if (!isInitialized || soundConfig == null) return;
        PlaySound3D(soundConfig.jumpSound, position);
    }

    public void PlayLanding(Vector3 position)
    {
        if (!isInitialized || soundConfig == null) return;
        PlaySound3D(soundConfig.landingSound, position);
    }

    // INTERACTION AUDIO
    public void PlayPickupAmmo(Vector3 position)
    {
        if (!isInitialized || soundConfig == null) return;
        PlaySound3D(soundConfig.pickupAmmo, position);
    }

    public void PlayPickupOxygen(Vector3 position)
    {
        if (!isInitialized || soundConfig == null) return;
        PlaySound3D(soundConfig.pickupOxygen, position);
    }

    // ZONE AUDIO
    public void PlayChipPlacement(Vector2Int zonePosition)
    {
        if (!isInitialized || soundConfig == null) return;

        Vector3 worldPos = Vector3.zero;
        if (TileManager.Instance != null)
        {
            worldPos = TileManager.Instance.GridToWorldPosition(zonePosition);
        }
        else
        {
            worldPos = new Vector3(zonePosition.x * 20f, 0f, zonePosition.y * 20f);
        }

        PlaySound3D(soundConfig.chipPlacement, worldPos);
        StartCountdownSequence();
    }

    public void PlayZoneComplete(Vector2Int zonePosition)
    {
        if (!isInitialized || soundConfig == null) return;

        StopCountdownSequence();

        Vector3 worldPos = Vector3.zero;
        if (TileManager.Instance != null)
        {
            worldPos = TileManager.Instance.GridToWorldPosition(zonePosition);
        }
        else
        {
            worldPos = new Vector3(zonePosition.x * 20f, 0f, zonePosition.y * 20f);
        }

        PlaySound3D(soundConfig.zoneComplete, worldPos);
    }

    // COUNTDOWN SYSTEM
    private void StartCountdownSequence()
    {
        if (!isInitialized || isCountdownActive || soundConfig == null) return;

        isCountdownActive = true;

        if (soundConfig.countdownMusic.clip != null)
        {
            PlayMusic(soundConfig.countdownMusic);
        }

        countdownCoroutine = StartCoroutine(CountdownSequence());
    }

    private IEnumerator CountdownSequence()
    {
        float countdownDuration = 30f;

        for (int i = (int)countdownDuration; i > 0; i--)
        {
            if (!isCountdownActive) yield break;

            if (i <= 5 && soundConfig.countdownFinal.clip != null)
            {
                PlaySFX(soundConfig.countdownFinal);
            }
            else if (i <= 10 && soundConfig.countdownTick.clip != null)
            {
                PlaySFX(soundConfig.countdownTick);
            }

            yield return new WaitForSeconds(1f);
        }

        isCountdownActive = false;
    }

    private void StopCountdownSequence()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        isCountdownActive = false;
        PlayAmbientMusic();
    }

    // COMBAT AUDIO
    public void PlayWeaponSound(int weaponIndex, Vector3 position)
    {
        if (!isInitialized || soundConfig == null) return;

        AudioClipData[] weaponSounds = weaponIndex == 0 ? soundConfig.weapon1Sounds : soundConfig.weapon2Sounds;

        if (weaponSounds != null && weaponSounds.Length > 0)
        {
            AudioClipData weaponSound = weaponSounds[Random.Range(0, weaponSounds.Length)];
            PlaySound3D(weaponSound, position);
        }
    }

    public void PlayImpactSound(Vector3 position)
    {
        if (!isInitialized || soundConfig == null) return;

        if (soundConfig.impactSounds != null && soundConfig.impactSounds.Length > 0)
        {
            AudioClipData impactSound = soundConfig.impactSounds[Random.Range(0, soundConfig.impactSounds.Length)];
            PlaySound3D(impactSound, position);
        }
    }

    public void PlayWeaponEmpty(Vector3 position)
    {
        if (!isInitialized || soundConfig == null) return;
        if (soundConfig.weaponEmpty.clip != null)
        {
            PlaySound3D(soundConfig.weaponEmpty, position);
        }
    }

    public void PlayReloadSound(Vector3 position)
    {
        if (!isInitialized || soundConfig == null) return;
        if (soundConfig.weaponReload.clip != null)
        {
            PlaySound3D(soundConfig.weaponReload, position);
        }
    }

    // MISSION AUDIO
    public void PlayHoverSound(Vector3 position)
    {
        if (!isInitialized || soundConfig == null) return;
        PlaySound3D(soundConfig.hoverSound, position);
    }

    public void PlayLandingSequence()
    {
        if (!isInitialized || soundConfig == null) return;
        PlaySFX(soundConfig.landingSequence);
    }

    public void PlayExtractionMusic()
    {
        if (!isInitialized || soundConfig == null) return;
        PlayMusic(soundConfig.extractionMusic);
    }

    // DIALOGUE
    public void PlayMissionStart()
    {
        if (!isInitialized || soundConfig == null) return;
        PlayVoice(soundConfig.missionStart);
    }

    public void PlayMissionConfirm()
    {
        if (!isInitialized || soundConfig == null) return;
        PlayVoice(soundConfig.missionConfirm);
    }

    public void PlayZoneUpdate()
    {
        if (!isInitialized || soundConfig == null) return;
        PlayVoice(soundConfig.zoneUpdate);
    }

    public void PlayExtractionCall()
    {
        if (!isInitialized || soundConfig == null) return;
        PlayVoice(soundConfig.extractionCall);
    }

    // MUSIC
    public void PlayGameStartMusic()
    {
        if (!isInitialized || soundConfig == null) return;
        PlayMusic(soundConfig.gameStartMusic);
    }

    public void PlayCreditsMusic()
    {
        if (!isInitialized || soundConfig == null) return;
        PlayMusic(soundConfig.creditsMusic);
    }

    public void PlayAmbientMusic()
    {
        if (!isInitialized || soundConfig == null) return;
        if (soundConfig.ambientMusic.clip != null)
        {
            PlayMusic(soundConfig.ambientMusic, ambientSource);
        }
    }

    // CORE PLAYBACK METHODS with Enhanced Audio Contro
    private void PlayMusic(AudioClipData audioData, AudioSource source = null)
    {
        if (audioData.clip == null) return;

        if (source == null) source = musicSource;
        if (source == null) return;

        source.clip = audioData.clip;
        source.volume = audioData.volume;
        source.pitch = audioData.pitch;
        source.loop = audioData.loop;
        source.time = audioData.StartTime; // Use custom start time
        source.Play();

        // Handle custom end time
        if (audioData.EndTime < audioData.clip.length)
        {
            StartCoroutine(StopAudioAtTime(source, audioData.ClipDuration));
        }
    }

    private void PlaySFX(AudioClipData audioData)
    {
        if (audioData.clip == null || sfxSource == null) return;

        sfxSource.pitch = audioData.pitch;

        // Handle custom timing for SFX
        if (audioData.StartTime > 0f || audioData.EndTime < audioData.clip.length)
        {
            StartCoroutine(PlaySFXWithCustomTiming(audioData));
        }
        else
        {
            sfxSource.PlayOneShot(audioData.clip, audioData.volume);
        }
    }

    private IEnumerator PlaySFXWithCustomTiming(AudioClipData audioData)
    {
        sfxSource.clip = audioData.clip;
        sfxSource.volume = audioData.volume;
        sfxSource.time = audioData.StartTime;
        sfxSource.Play();

        yield return new WaitForSeconds(audioData.ClipDuration);

        sfxSource.Stop();
    }

    private void PlayVoice(AudioClipData audioData)
    {
        if (audioData.clip == null || voiceSource == null) return;

        voiceSource.clip = audioData.clip;
        voiceSource.volume = audioData.volume;
        voiceSource.pitch = audioData.pitch;
        voiceSource.time = audioData.StartTime; // Use custom start time
        voiceSource.Play();

        // Handle custom end time
        if (audioData.EndTime < audioData.clip.length)
        {
            StartCoroutine(StopAudioAtTime(voiceSource, audioData.ClipDuration));
        }
    }

    private void PlaySound3D(AudioClipData audioData, Vector3 position)
    {
        if (audioData.clip == null) return;

        AudioSource source = GetPooledAudioSource();
        if (source == null) return;

        source.transform.position = position;
        source.clip = audioData.clip;
        source.volume = audioData.volume;
        source.pitch = audioData.pitch;
        source.loop = audioData.loop;
        source.spatialBlend = audioData.is3D ? 1f : 0f;
        source.time = audioData.StartTime; // Use custom start time

        source.Play();

        if (!audioData.loop)
        {
            float playDuration = audioData.ClipDuration;
            StartCoroutine(ReturnToPool(source, playDuration));
        }
    }

    private IEnumerator StopAudioAtTime(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (source != null && source.isPlaying)
        {
            source.Stop();
        }
    }

    private AudioSource GetPooledAudioSource()
    {
        if (audioSourcePool.Count > 0)
        {
            AudioSource source = audioSourcePool.Dequeue();
            activeAudioSources.Add(source);
            return source;
        }

        if (activeAudioSources.Count > 0)
        {
            AudioSource source = activeAudioSources[0];
            activeAudioSources.RemoveAt(0);
            activeAudioSources.Add(source);
            source.Stop();
            return source;
        }

        return null;
    }

    private IEnumerator ReturnToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (activeAudioSources.Contains(source))
        {
            activeAudioSources.Remove(source);
            source.Stop();
            source.clip = null;
            audioSourcePool.Enqueue(source);
        }
    }

    // VOLUME CONTROL
    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
    }

    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = volume * 0.7f;
        if (ambientSource != null) ambientSource.volume = volume * 0.4f;
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = volume * 0.8f;
    }

    public void SetVoiceVolume(float volume)
    {
        if (voiceSource != null)
        {
            voiceSource.volume = Mathf.Clamp01(volume) * 0.9f;
        }
    }

    // PUBLIC INTERFACE
    public bool IsSystemReady()
    {
        return isInitialized && soundConfig != null;
    }

    public void ForceEventSubscription()
    {
        SubscribeToEvents();
    }

    // DEBUG
    [ContextMenu("Test All Sounds")]
    private void TestAllSounds()
    {
        if (!Application.isPlaying || !IsSystemReady()) return;

        Vector3 testPos = transform.position;
        StartFootsteps(false, testPos);
        PlayJump(testPos);
        PlayPickupAmmo(testPos);
        PlayMissionStart();
    }

    [ContextMenu("Stop All Continuous Sounds")]
    private void StopAllContinuousSounds()
    {
        StopFootsteps();
    }

    [ContextMenu("Validate Sound Configuration")]
    private void ValidateSoundConfiguration()
    {
        if (soundConfig == null)
        {
            Debug.LogError("SoundConfiguration is null!");
            return;
        }

        bool isValid = soundConfig.ValidateConfiguration();
        Debug.Log(isValid ? "Sound configuration valid" : "Some audio clips missing");
    }

    private void OnDestroy()
    {
        if (TileManager.Instance != null)
        {
            try
            {
                TileManager.Instance.OnZoneActivationStarted -= PlayChipPlacement;
                TileManager.Instance.OnZoneActivationComplete -= PlayZoneComplete;
            }
            catch (System.Exception)
            {
                // Safe cleanup
            }
        }
    }
}