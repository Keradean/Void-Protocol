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

    [Header("Timeline Control - Managed by Custom Editor")]
    [SerializeField] private float startTime;
    [SerializeField] private float endTime;
    [Range(0f, 1f)] public float fadeInDuration;
    [Range(0f, 1f)] public float fadeOutDuration;

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

[System.Serializable]
public struct GameDialogue
{
    public string dialogueID;
    public AudioClipData clip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Movement Audio")]
    [SerializeField] private AudioClipData walkSound;
    [SerializeField] private AudioClipData runSound;
    [SerializeField] private AudioClipData jumpSound;
    [SerializeField] private AudioClipData landingSound;

    [Header("Enemy Audio")]
    [SerializeField] private AudioClipData spiderMovement;
    [SerializeField] private AudioClipData spiderAttack;
    [SerializeField] private AudioClipData spiderDefeat;

    [Header("Combat Audio")]
    [SerializeField] private AudioClipData[] weapon1Sounds;
    [SerializeField] private AudioClipData[] weapon2Sounds;
    [SerializeField] private AudioClipData[] impactSoundObjects;
    [SerializeField] private AudioClipData[] impactSoundMobs;
    [SerializeField] private AudioClipData weaponReload;
    [SerializeField] private AudioClipData weaponEmpty;

    [Header("Interaction Audio")]
    [SerializeField] private AudioClipData pickupItem;
    [SerializeField] private AudioClipData mediPenUse;
    [SerializeField] private AudioClipData doorOpensClose;
    [SerializeField] private AudioClipData terminalActivation;
    [SerializeField] private AudioClipData countdownTick;
    [SerializeField] private AudioClipData zoneCompletion;
    [SerializeField] private AudioClipData finalCompletionSignal;

    [Header("Warning System")]
    [SerializeField] private AudioClipData lowOxygenWarning;
    [SerializeField] private AudioClipData staminaWarning;
    [SerializeField] private AudioClipData healthWarning;

    [Header("Hovercraft Category")]
    [SerializeField] private AudioClipData landingSequence;
    [SerializeField] private AudioClipData extractionSequence;

    [Header("Dialogue Audio")]
    [SerializeField] private GameDialogue[] gameDialogues;
    [SerializeField] private AudioClipData missionStart;
    [SerializeField] private AudioClipData missionConfirm;
    [SerializeField] private AudioClipData zoneUpdate;
    [SerializeField] private AudioClipData extractionCall;

    [Header("Ambient Music")]
    [SerializeField] private AudioClipData[] ambientMusicTracks;

    [Header("Game Music")]
    [SerializeField] private AudioClipData gameStartMusic;
    [SerializeField] private AudioClipData creditsMusic;
    [SerializeField] private AudioClipData extractionMusic;

    [Header("3D Audio Settings")]
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    [Header("Performance")]
    [SerializeField] private int maxConcurrentSounds = 16;
    [SerializeField] private bool enableObjectPooling = true;

    // Audio Sources - Internal Management Only
    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource voiceSource;
    private AudioSource ambientSource;
    private AudioSource continuousFootstepSource;

    // Audio pooling and state management
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private List<AudioSource> activeAudioSources = new List<AudioSource>();
    private Coroutine countdownCoroutine;
    private bool isCountdownActive = false;
    private bool isFootstepPlaying = false;
    private bool isInitialized = false;
    private int currentAmbientTrack = 0;

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

    private void Start()
    {
        InitializeAudioSources();
        InitializeAudioPool();
        StartCoroutine(DelayedInitialization());
    }

    private IEnumerator DelayedInitialization()
    {
        yield return new WaitForSeconds(0.1f);

        SubscribeToEvents();
        isInitialized = true;

        if (ambientMusicTracks != null && ambientMusicTracks.Length > 0)
        {
            PlayAmbientMusic();
        }
    }

    private void InitializeAudioSources()
    {
        musicSource = CreateAudioSource("MusicSource", 0.7f, true);
        sfxSource = CreateAudioSource("SFXSource", 0.8f, false);
        voiceSource = CreateAudioSource("VoiceSource", 0.9f, false);
        ambientSource = CreateAudioSource("AmbientSource", 0.4f, true);
        continuousFootstepSource = CreateAudioSource("FootstepSource", 0.6f, true, true);
    }

    private AudioSource CreateAudioSource(string name, float volume, bool loop, bool is3D = false)
    {
        GameObject sourceObj = new GameObject(name);
        sourceObj.transform.SetParent(transform);

        AudioSource source = sourceObj.AddComponent<AudioSource>();
        source.volume = volume;
        source.loop = loop;
        source.playOnAwake = false;
        source.spatialBlend = is3D ? 1f : 0f;

        if (is3D)
        {
            source.rolloffMode = rolloffMode;
            source.maxDistance = maxDistance;
        }

        return source;
    }

    private void InitializeAudioPool()
    {
        if (!enableObjectPooling) return;

        for (int i = 0; i < maxConcurrentSounds; i++)
        {
            GameObject audioObj = new GameObject("PooledAudioSource_" + i);
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
            TileManager.Instance.OnZoneActivationStarted += PlayTerminalActivation;
            TileManager.Instance.OnZoneActivationComplete += PlayZoneCompletion;
        }
    }

    // ===== MOVEMENT AUDIO =====
    public void StartFootsteps(bool isRunning, Vector3 position)
    {
        if (!isInitialized) return;

        AudioClipData foot = isRunning ? runSound : walkSound;
        if (foot.clip == null) return;

        continuousFootstepSource.clip = foot.clip;
        continuousFootstepSource.volume = foot.volume;
        continuousFootstepSource.pitch = isRunning ? 1.5f : 1.0f;
        continuousFootstepSource.time = foot.StartTime;
        continuousFootstepSource.transform.position = position;

        if (!isFootstepPlaying)
        {
            continuousFootstepSource.Play();
            isFootstepPlaying = true;
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

    public void PlayJump(Vector3 position)
    {
        if (!isInitialized || jumpSound.clip == null) return;
        PlaySound3D(jumpSound, position);
    }

    public void PlayLanding(Vector3 position)
    {
        if (!isInitialized || landingSound.clip == null) return;
        PlaySound3D(landingSound, position);
    }

    // ===== ENEMY AUDIO =====
    public void PlaySpiderMovement(Vector3 position)
    {
        if (!isInitialized || spiderMovement.clip == null) return;
        PlaySound3D(spiderMovement, position);
    }

    public void PlaySpiderAttack(Vector3 position)
    {
        if (!isInitialized || spiderAttack.clip == null) return;
        PlaySound3D(spiderAttack, position);
    }

    public void PlaySpiderDefeat(Vector3 position)
    {
        if (!isInitialized || spiderDefeat.clip == null) return;
        PlaySound3D(spiderDefeat, position);
    }

    // ===== COMBAT AUDIO =====
    public void PlayWeaponSound(int weaponIndex, Vector3 position)
    {
        if (!isInitialized) return;

        AudioClipData[] weaponSounds = weaponIndex == 0 ? weapon1Sounds : weapon2Sounds;

        if (weaponSounds != null && weaponSounds.Length > 0)
        {
            AudioClipData weaponSound = weaponSounds[Random.Range(0, weaponSounds.Length)];
            if (weaponSound.clip != null)
                PlaySound3D(weaponSound, position);
        }
    }

    public void PlayImpactSoundObjects(Vector3 position)
    {
        if (!isInitialized) return;

        if (impactSoundObjects != null && impactSoundObjects.Length > 0)
        {
            AudioClipData impactSound = impactSoundObjects[Random.Range(0, impactSoundObjects.Length)];
            if (impactSound.clip != null)
                PlaySound3D(impactSound, position);
        }
    }

    public void PlayImpactSoundMobs(Vector3 position)
    {
        if (!isInitialized) return;

        if (impactSoundMobs != null && impactSoundMobs.Length > 0)
        {
            AudioClipData impactSound = impactSoundMobs[Random.Range(0, impactSoundMobs.Length)];
            if (impactSound.clip != null)
                PlaySound3D(impactSound, position);
        }
    }

    public void PlayWeaponEmpty(Vector3 position)
    {
        if (!isInitialized || weaponEmpty.clip == null) return;
        PlaySound3D(weaponEmpty, position);
    }

    public void PlayReloadSound(Vector3 position)
    {
        if (!isInitialized || weaponReload.clip == null) return;
        PlaySound3D(weaponReload, position);
    }

    // ===== INTERACTION AUDIO =====
    public void PlayPickupItem(Vector3 position)
    {
        if (!isInitialized || pickupItem.clip == null) return;
        PlaySound3D(pickupItem, position);
    }

    public void PlayMediPenUse(Vector3 position)
    {
        if (!isInitialized || mediPenUse.clip == null) return;
        PlaySound3D(mediPenUse, position);
    }

    public void PlayDoorOpensClose(Vector3 position)
    {
        if (!isInitialized || doorOpensClose.clip == null) return;
        PlaySound3D(doorOpensClose, position);
    }

    public void PlayTerminalActivation(Vector2Int zonePosition)
    {
        if (!isInitialized || terminalActivation.clip == null) return;

        Vector3 worldPos = TileManager.Instance != null
            ? TileManager.Instance.GridToWorldPosition(zonePosition)
            : new Vector3(zonePosition.x * 20f, 0f, zonePosition.y * 20f);

        PlaySound3D(terminalActivation, worldPos);
        StartCountdownSequence();
    }

    public void PlayCountdownTick()
    {
        if (!isInitialized || countdownTick.clip == null) return;
        PlaySFX(countdownTick);
    }

    public void PlayZoneCompletion(Vector2Int zonePosition)
    {
        if (!isInitialized || zoneCompletion.clip == null) return;

        StopCountdownSequence();

        Vector3 worldPos = TileManager.Instance != null
            ? TileManager.Instance.GridToWorldPosition(zonePosition)
            : new Vector3(zonePosition.x * 20f, 0f, zonePosition.y * 20f);

        PlaySound3D(zoneCompletion, worldPos);
    }

    public void PlayFinalCompletionSignal(Vector3 position)
    {
        if (!isInitialized || finalCompletionSignal.clip == null) return;
        PlaySound3D(finalCompletionSignal, position);
    }

    // ===== WARNING SYSTEM =====
    public void PlayLowOxygenWarning(Vector3 position = default)
    {
        if (!isInitialized || lowOxygenWarning.clip == null) return;

        if (position == default)
            PlaySFX(lowOxygenWarning);
        else
            PlaySound3D(lowOxygenWarning, position);
    }

    public void PlayStaminaWarning(Vector3 position = default)
    {
        if (!isInitialized || staminaWarning.clip == null) return;

        if (position == default)
            PlaySFX(staminaWarning);
        else
            PlaySound3D(staminaWarning, position);
    }

    public void PlayHealthWarning(Vector3 position = default)
    {
        if (!isInitialized || healthWarning.clip == null) return;

        if (position == default)
            PlaySFX(healthWarning);
        else
            PlaySound3D(healthWarning, position);
    }

    // ===== HOVERCRAFT CATEGORY =====
    public void PlayLandingSequence(Vector3 position)
    {
        if (!isInitialized || landingSequence.clip == null) return;
        PlaySound3D(landingSequence, position);
    }

    public void PlayExtractionSequence(Vector3 position)
    {
        if (!isInitialized || extractionSequence.clip == null) return;
        PlaySound3D(extractionSequence, position);
    }

    // ===== DIALOGUE AUDIO =====
    public void PlayDialogue(string dialogueID, Vector3 position = default)
    {
        if (!isInitialized || gameDialogues == null) return;

        var dialogue = System.Array.Find(gameDialogues, d => d.dialogueID == dialogueID);
        if (dialogue.clip.clip != null)
        {
            if (position == default)
                PlayVoice(dialogue.clip);
            else
                PlaySound3D(dialogue.clip, position);
        }
    }

    public void PlayMissionStart()
    {
        if (!isInitialized || missionStart.clip == null) return;
        PlayVoice(missionStart);
    }

    public void PlayMissionConfirm()
    {
        if (!isInitialized || missionConfirm.clip == null) return;
        PlayVoice(missionConfirm);
    }

    public void PlayZoneUpdate()
    {
        if (!isInitialized || zoneUpdate.clip == null) return;
        PlayVoice(zoneUpdate);
    }

    public void PlayExtractionCall()
    {
        if (!isInitialized || extractionCall.clip == null) return;
        PlayVoice(extractionCall);
    }

    // ===== AMBIENT MUSIC =====
    public void PlayAmbientMusic()
    {
        if (!isInitialized || ambientMusicTracks == null || ambientMusicTracks.Length == 0) return;

        if (currentAmbientTrack >= ambientMusicTracks.Length)
            currentAmbientTrack = 0;

        AudioClipData currentTrack = ambientMusicTracks[currentAmbientTrack];
        if (currentTrack.clip != null)
        {
            PlayMusic(currentTrack, ambientSource);
            StartCoroutine(PlayNextAmbientTrack(currentTrack.ClipDuration));
        }

        currentAmbientTrack++;
    }

    private IEnumerator PlayNextAmbientTrack(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayAmbientMusic();
    }

    // ===== GAME MUSIC =====
    public void PlayGameStartMusic()
    {
        if (!isInitialized || gameStartMusic.clip == null) return;
        PlayMusic(gameStartMusic);
    }

    public void PlayCreditsMusic()
    {
        if (!isInitialized || creditsMusic.clip == null) return;
        PlayMusic(creditsMusic);
    }

    public void PlayExtractionMusic()
    {
        if (!isInitialized || extractionMusic.clip == null) return;
        PlayMusic(extractionMusic);
    }

    // ===== COUNTDOWN SYSTEM =====
    private void StartCountdownSequence()
    {
        if (!isInitialized || isCountdownActive) return;

        isCountdownActive = true;
        countdownCoroutine = StartCoroutine(CountdownSequence());
    }

    private IEnumerator CountdownSequence()
    {
        float countdownDuration = 30f;

        for (int i = (int)countdownDuration; i > 0; i--)
        {
            if (!isCountdownActive) yield break;

            if (i <= 10)
            {
                PlayCountdownTick();
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
    }

    // ===== CORE AUDIO METHODS =====
    private void PlayMusic(AudioClipData audioData, AudioSource source = null)
    {
        if (audioData.clip == null) return;

        if (source == null) source = musicSource;
        if (source == null) return;

        source.clip = audioData.clip;
        source.volume = audioData.volume;
        source.pitch = audioData.pitch;
        source.loop = audioData.loop;
        source.time = audioData.StartTime;
        source.Play();

        if (audioData.ClipDuration < audioData.clip.length)
        {
            StartCoroutine(StopAudioAtTime(source, audioData.ClipDuration));
        }
    }

    private void PlaySFX(AudioClipData audioData)
    {
        if (audioData.clip == null || sfxSource == null) return;

        sfxSource.pitch = audioData.pitch;

        if (audioData.StartTime > 0f || audioData.EndTime < audioData.clip.length)
        {
            StartCoroutine(PlaySFXWithCustomTiming(audioData));
        }
        else
        {
            sfxSource.PlayOneShot(audioData.clip, audioData.volume);
        }
    }

    private void PlayVoice(AudioClipData audioData)
    {
        if (audioData.clip == null || voiceSource == null) return;

        voiceSource.clip = audioData.clip;
        voiceSource.volume = audioData.volume;
        voiceSource.pitch = audioData.pitch;
        voiceSource.time = audioData.StartTime;
        voiceSource.Play();

        if (audioData.ClipDuration < audioData.clip.length)
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
        source.time = audioData.StartTime;

        source.Play();

        if (!audioData.loop)
        {
            float playDuration = audioData.ClipDuration;
            StartCoroutine(ReturnToPool(source, playDuration));
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

    private IEnumerator StopAudioAtTime(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (source != null && source.isPlaying)
        {
            source.Stop();
        }
    }

    private IEnumerator PlaySFXWithCustomTiming(AudioClipData audioData)
    {
        sfxSource.clip = audioData.clip;
        sfxSource.volume = audioData.volume;
        sfxSource.time = audioData.StartTime;
        sfxSource.Play();

        yield return new WaitForSeconds(audioData.ClipDuration);

        if (sfxSource.isPlaying && sfxSource.clip == audioData.clip)
        {
            sfxSource.Stop();
        }
    }

    // ===== VOLUME CONTROL =====
    public void SetMasterVolume(float volume) => AudioListener.volume = Mathf.Clamp01(volume);

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
        if (continuousFootstepSource != null) continuousFootstepSource.volume = volume * 0.6f;
    }

    public void SetVoiceVolume(float volume)
    {
        if (voiceSource != null)
            voiceSource.volume = Mathf.Clamp01(volume) * 0.9f;
    }

    // ===== LEGACY COMPATIBILITY =====
    [System.Obsolete("Use PlayPickupItem instead")]
    public void PlayPickupAmmo(Vector3 position) => PlayPickupItem(position);

    [System.Obsolete("Use PlayPickupItem instead")]
    public void PlayPickupOxygen(Vector3 position) => PlayPickupItem(position);

    [System.Obsolete("Use PlayImpactSoundObjects or PlayImpactSoundMobs instead")]
    public void PlayImpactSound(Vector3 position) => PlayImpactSoundObjects(position);

    // ===== CLEANUP =====
    private void OnDestroy()
    {
        if (TileManager.Instance != null)
        {
            try
            {
                TileManager.Instance.OnZoneActivationStarted -= PlayTerminalActivation;
                TileManager.Instance.OnZoneActivationComplete -= PlayZoneCompletion;
            }
            catch { }
        }
    }
}