using UnityEngine;

/// <summary>
/// Global ambient audio to prevent dead silence:
/// - Room tone loop (or procedural fallback if missing)
/// - Optional music loop (loaded from Resources by default)
///
/// This component bootstraps itself at runtime and persists across scenes.
/// </summary>
public class AmbientSoundscape : MonoBehaviour
{
    public static AmbientSoundscape Instance { get; private set; }

    [Header("Resources (optional auto-load)")]
    [Tooltip("If Music Loop is empty, we try to load this from Resources.")]
    public string musicResourcesPath = "Audio/Ambient/AmbientMusic";

    [Tooltip("If Room Tone Loop is empty, we try to load this from Resources.")]
    public string roomToneResourcesPath = "Audio/Ambient/RoomTone";

    [Header("Room Tone")]
    public bool enableRoomTone = true;
    public AudioClip roomToneLoop;
    [Range(0f, 1f)] public float roomToneVolume = 0.08f;

    [Tooltip("If no room tone clip is provided or found in Resources, generate a subtle procedural loop.")]
    public bool generateProceduralRoomToneIfMissing = true;

    [Header("Music")]
    public bool enableMusic = true;
    public AudioClip musicLoop;
    [Range(0f, 1f)] public float musicVolume = 0.12f;

    [Header("Fade")]
    [Tooltip("Simple fade-in time for room tone and music (seconds).")]
    public float fadeInTime = 0.5f;

    private AudioSource _roomToneSource;
    private AudioSource _musicSource;

    private float _roomToneTargetVol;
    private float _musicTargetVol;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        // Ensure one instance exists without requiring scene edits
        if (FindFirstObjectByType<AmbientSoundscape>() != null) return;

        var go = new GameObject("AmbientSoundscape");
        go.AddComponent<AmbientSoundscape>();
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

        _roomToneSource = gameObject.AddComponent<AudioSource>();
        _musicSource = gameObject.AddComponent<AudioSource>();

        Configure2DLoopSource(_roomToneSource);
        Configure2DLoopSource(_musicSource);
    }

    private void Start()
    {
        TryLoadClipsFromResources();

        if (enableRoomTone && roomToneLoop == null && generateProceduralRoomToneIfMissing)
        {
            roomToneLoop = CreateProceduralRoomTone(loopSeconds: 6f, baseVolume: 0.05f);
        }

        ApplyAndPlay();
    }

    private void Update()
    {
        // Simple fade-in toward target volumes
        if (fadeInTime <= 0f)
        {
            if (_roomToneSource != null) _roomToneSource.volume = _roomToneTargetVol;
            if (_musicSource != null) _musicSource.volume = _musicTargetVol;
            return;
        }

        float t = Time.unscaledDeltaTime / fadeInTime;
        if (_roomToneSource != null) _roomToneSource.volume = Mathf.MoveTowards(_roomToneSource.volume, _roomToneTargetVol, t);
        if (_musicSource != null) _musicSource.volume = Mathf.MoveTowards(_musicSource.volume, _musicTargetVol, t);
    }

    private void TryLoadClipsFromResources()
    {
        if (musicLoop == null && !string.IsNullOrWhiteSpace(musicResourcesPath))
        {
            musicLoop = Resources.Load<AudioClip>(musicResourcesPath);
        }

        if (roomToneLoop == null && !string.IsNullOrWhiteSpace(roomToneResourcesPath))
        {
            roomToneLoop = Resources.Load<AudioClip>(roomToneResourcesPath);
        }
    }

    private void ApplyAndPlay()
    {
        // Room tone
        if (enableRoomTone && roomToneLoop != null)
        {
            _roomToneSource.clip = roomToneLoop;
            _roomToneSource.loop = true;
            _roomToneSource.Play();
            _roomToneTargetVol = roomToneVolume;
        }
        else
        {
            _roomToneSource.Stop();
            _roomToneSource.clip = null;
            _roomToneTargetVol = 0f;
        }

        // Music
        if (enableMusic && musicLoop != null)
        {
            _musicSource.clip = musicLoop;
            _musicSource.loop = true;
            _musicSource.Play();
            _musicTargetVol = musicVolume;
        }
        else
        {
            _musicSource.Stop();
            _musicSource.clip = null;
            _musicTargetVol = 0f;
        }
    }

    private static void Configure2DLoopSource(AudioSource src)
    {
        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 0f; // 2D
        src.dopplerLevel = 0f;
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        src.volume = 0f;
    }

    private static AudioClip CreateProceduralRoomTone(float loopSeconds, float baseVolume)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int channels = 1;
        int totalSamples = Mathf.Max(1, Mathf.RoundToInt(loopSeconds * sampleRate));
        float[] data = new float[totalSamples * channels];

        // Subtle filtered noise + a very low sine to avoid "pure hiss"
        float noise = 0f;
        float noiseAlpha = 0.02f; // lower = smoother
        float sineFreq = 42f;

        for (int i = 0; i < totalSamples; i++)
        {
            float white = (Random.value * 2f - 1f);
            noise = Mathf.Lerp(noise, white, noiseAlpha);

            float t = i / (float)sampleRate;
            float sine = Mathf.Sin(2f * Mathf.PI * sineFreq * t) * 0.15f;

            float sample = (noise * 0.85f + sine) * baseVolume;
            data[i] = Mathf.Clamp(sample, -0.2f, 0.2f);
        }

        var clip = AudioClip.Create("ProceduralRoomTone", totalSamples, channels, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}


