using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Optional: enable if you want to migrate old fields walkSound1/runSound1 automatically
// using UnityEngine.Serialization;

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

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource continuousFootstepSource;

    [Header("Movement Audio - Single Walk/Run")]
    // [FormerlySerializedAs("walkSound1")]
    [SerializeField] private AudioClipData walkSound;
    // [FormerlySerializedAs("runSound1")]
    [SerializeField] private AudioClipData runSound;
    [SerializeField] private AudioClipData jumpSound;
    [SerializeField] private AudioClipData landingSound;

    [Header("Interaction Audio")]
    [SerializeField] private AudioClipData pickupAmmo;
    [SerializeField] private AudioClipData pickupOxygen;
    [SerializeField] private AudioClipData chipPlacement;
    [SerializeField] private AudioClipData zoneComplete;

    [Header("Combat Audio")]
    [SerializeField] private AudioClipData[] weapon1Sounds;
    [SerializeField] private AudioClipData[] weapon2Sounds;
    [SerializeField] private AudioClipData[] impactSounds;
    [SerializeField] private AudioClipData weaponReload;
    [SerializeField] private AudioClipData weaponEmpty;

    [Header("Music & Ambience")]
    [SerializeField] private AudioClipData gameStartMusic;
    [SerializeField] private AudioClipData ambientMusic;
    [SerializeField] private AudioClipData creditsMusic;

    [Header("Countdown System")]
    [SerializeField] private AudioClipData countdownTick;
    [SerializeField] private AudioClipData countdownMusic;
    [SerializeField] private AudioClipData countdownFinal;

    [Header("3D Audio Settings")]
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    [Header("Performance")]
    [SerializeField] private int maxConcurrentSounds = 16;
    [SerializeField] private bool enableObjectPooling = true;

    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private List<AudioSource> activeAudioSources = new List<AudioSource>();
    private Coroutine countdownCoroutine;
    private bool isCountdownActive = false;
    private bool isFootstepPlaying = false;
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

        if (ambientMusic.clip != null)
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
            TileManager.Instance.OnZoneActivationStarted += PlayChipPlacement;
            TileManager.Instance.OnZoneActivationComplete += PlayZoneComplete;
        }
    }

    // Movement
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

    // Single-shot SFX
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

    public void PlayPickupAmmo(Vector3 position)
    {
        if (!isInitialized || pickupAmmo.clip == null) return;
        PlaySound3D(pickupAmmo, position);
    }

    public void PlayPickupOxygen(Vector3 position)
    {
        if (!isInitialized || pickupOxygen.clip == null) return;
        PlaySound3D(pickupOxygen, position);
    }

    // Zones
    public void PlayChipPlacement(Vector2Int zonePosition)
    {
        if (!isInitialized || chipPlacement.clip == null) return;

        Vector3 worldPos = TileManager.Instance != null
            ? TileManager.Instance.GridToWorldPosition(zonePosition)
            : new Vector3(zonePosition.x * 20f, 0f, zonePosition.y * 20f);

        PlaySound3D(chipPlacement, worldPos);
        StartCountdownSequence();
    }

    public void PlayZoneComplete(Vector2Int zonePosition)
    {
        if (!isInitialized || zoneComplete.clip == null) return;

        StopCountdownSequence();

        Vector3 worldPos = TileManager.Instance != null
            ? TileManager.Instance.GridToWorldPosition(zonePosition)
            : new Vector3(zonePosition.x * 20f, 0f, zonePosition.y * 20f);

        PlaySound3D(zoneComplete, worldPos);
    }

    // Countdown
    private void StartCountdownSequence()
    {
        if (!isInitialized || isCountdownActive) return;

        isCountdownActive = true;

        if (countdownMusic.clip != null)
        {
            PlayMusic(countdownMusic);
        }

        countdownCoroutine = StartCoroutine(CountdownSequence());
    }

    private IEnumerator CountdownSequence()
    {
        float countdownDuration = 30f;

        for (int i = (int)countdownDuration; i > 0; i--)
        {
            if (!isCountdownActive) yield break;

            if (i <= 5 && countdownFinal.clip != null)
            {
                PlaySFX(countdownFinal);
            }
            else if (i <= 10 && countdownTick.clip != null)
            {
                PlaySFX(countdownTick);
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

    // Combat
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

    public void PlayImpactSound(Vector3 position)
    {
        if (!isInitialized) return;

        if (impactSounds != null && impactSounds.Length > 0)
        {
            AudioClipData impactSound = impactSounds[Random.Range(0, impactSounds.Length)];
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

    // Music control
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

    public void PlayAmbientMusic()
    {
        if (!isInitialized || ambientMusic.clip == null) return;
        PlayMusic(ambientMusic, ambientSource);
    }

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

    // Volume control
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

    [ContextMenu("Test Direct Audio Assignment")]
    private void TestDirectAudioAssignment()
    {
        if (!Application.isPlaying) return;

        Vector3 pos = transform.position;

        if (jumpSound.clip != null) PlayJump(pos);
        if (walkSound.clip != null)
        {
            StartFootsteps(false, pos);
        }
    }

    [ContextMenu("Validate Direct Assignment")]
    private void ValidateDirectAssignment()
    {
        int assigned = 0;
        int total = 0;

        if (walkSound.clip != null) assigned++; total++;
        if (runSound.clip != null) assigned++; total++;
        if (jumpSound.clip != null) assigned++; total++;
        if (landingSound.clip != null) assigned++; total++;

        Debug.Log("Assigned Clips Movement: " + assigned + "/" + total);
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
            catch { }
        }
    }

    // Defaults
    private void Reset() { ApplyDefaultStartingValues(); }

    [ContextMenu("Apply Default Starting Values")]
    private void ApplyDefaultStartingValues()
    {
        if (walkSound.clip == null) walkSound = AudioDefaults.Movement();
        if (runSound.clip == null) runSound = AudioDefaults.Movement();
        if (jumpSound.clip == null) jumpSound = AudioDefaults.Movement();
        if (landingSound.clip == null) landingSound = AudioDefaults.Movement();

        if (pickupAmmo.clip == null) pickupAmmo = AudioDefaults.Interaction3D();
        if (pickupOxygen.clip == null) pickupOxygen = AudioDefaults.Interaction3D();
        if (chipPlacement.clip == null) chipPlacement = AudioDefaults.Interaction3D();
        if (zoneComplete.clip == null) zoneComplete = AudioDefaults.Interaction3D();

        if (weaponReload.clip == null) weaponReload = AudioDefaults.Combat();
        if (weaponEmpty.clip == null) weaponEmpty = AudioDefaults.Combat();

        if (weapon1Sounds != null)
            for (int i = 0; i < weapon1Sounds.Length; i++)
                if (weapon1Sounds[i].clip == null) weapon1Sounds[i] = AudioDefaults.Combat();

        if (weapon2Sounds != null)
            for (int i = 0; i < weapon2Sounds.Length; i++)
                if (weapon2Sounds[i].clip == null) weapon2Sounds[i] = AudioDefaults.Combat();

        if (impactSounds != null)
            for (int i = 0; i < impactSounds.Length; i++)
                if (impactSounds[i].clip == null) impactSounds[i] = AudioDefaults.Combat();

        if (gameStartMusic.clip == null) gameStartMusic = AudioDefaults.Music();
        if (creditsMusic.clip == null) creditsMusic = AudioDefaults.Music();
        if (ambientMusic.clip == null) ambientMusic = AudioDefaults.Ambience();

        if (countdownTick.clip == null) countdownTick = AudioDefaults.CountdownTick();
        if (countdownMusic.clip == null) countdownMusic = AudioDefaults.CountdownMusic();
        if (countdownFinal.clip == null) countdownFinal = AudioDefaults.CountdownFinal();

#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private static class AudioDefaults
    {
        private static AudioClipData Base(float volume, float pitch, bool is3D, bool loop, float fadeIn, float fadeOut)
        {
            var d = new AudioClipData
            {
                clip = null,
                volume = Mathf.Clamp01(volume),
                pitch = Mathf.Clamp(pitch, 0.5f, 2.0f),
                is3D = is3D,
                loop = loop,
                fadeInDuration = Mathf.Max(0f, fadeIn),
                fadeOutDuration = Mathf.Max(0f, fadeOut),
            };
            d.StartTime = 0f;
            d.EndTime = 0f;
            return d;
        }

        public static AudioClipData Movement() { return Base(0.8f, 1.0f, true, false, 0.06f, 0.06f); }
        public static AudioClipData Interaction3D() { return Base(0.9f, 1.0f, true, false, 0.08f, 0.08f); }
        public static AudioClipData Combat() { return Base(0.95f, 1.0f, true, false, 0.08f, 0.10f); }
        public static AudioClipData Music() { return Base(0.8f, 1.0f, false, true, 1.5f, 2.5f); }
        public static AudioClipData Ambience() { return Base(0.5f, 1.0f, true, true, 0.8f, 2.0f); }
        public static AudioClipData CountdownTick() { return Base(0.8f, 1.0f, false, false, 0.06f, 0.06f); }
        public static AudioClipData CountdownMusic() { return Base(0.8f, 1.0f, false, false, 0.5f, 0.5f); }
        public static AudioClipData CountdownFinal() { return Base(1.0f, 1.0f, false, false, 0.05f, 0.08f); }
    }
}
