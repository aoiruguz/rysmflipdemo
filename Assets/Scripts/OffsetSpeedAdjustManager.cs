using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class OffsetSpeedAdjustManager : MonoBehaviour
{
    [Header("Settings")]
    public float noteTravelTime = 2.0f;
    public float globalOffset = 0f;

    [Header("Chart Settings")]
    public ChartData testChart;
    public bool useChartMode = false;
    public float chartLoopDuration = 22f;

    [Header("Spawn Settings")]
    public GameObject notePrefab;
    public float spawnY = 6f;
    public float judgmentLineY = -4f;
    public float laneX = 0f;
    public float spawnInterval = 2f;

    [Header("Audio")]
    public AudioClip hitSound;

    [Header("Visual Effects")]
    public Material particleMaterial;

    [Header("UI References")]
    public Text offsetValueText;
    public Text travelTimeValueText;
    public Button offsetIncreaseButton;
    public Button offsetDecreaseButton;
    public Button travelTimeIncreaseButton;
    public Button travelTimeDecreaseButton;

    [Header("Adjustment Steps")]
    public float offsetStep = 0.01f;
    public float travelTimeStep = 0.1f;

    private AudioSource musicSource;
    private AudioSource hitSoundSource;
    private float currentSpeed;
    private float nextSpawnTime;
    private bool isRunning = false;

    // Chart mode variables
    private double musicStartDspTime; // Use DSP time for precise audio timing
    private int currentNoteIndex = 0;
    private bool musicStarted = false;
    private List<GameObject> activeNotes = new List<GameObject>();

    void Awake()
    {
        // Create audio sources in Awake to ensure they exist before Start
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = false;

        hitSoundSource = gameObject.AddComponent<AudioSource>();
        hitSoundSource.playOnAwake = false;
        hitSoundSource.loop = false;
    }

    void Start()
    {
        // Load current settings
        globalOffset = GameSettings.GlobalOffset;

        // Calculate speed
        UpdateSpeed();

        // Setup UI
        UpdateUI();
        SetupButtons();

        // Configure audio sources
        musicSource.volume = GameSettings.MusicVolume;
        hitSoundSource.volume = GameSettings.NoteVolume;
        hitSoundSource.clip = hitSound;

        // Wait one frame before starting to ensure everything is initialized
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        // Wait for end of frame to ensure all initialization is complete
        yield return new WaitForEndOfFrame();

        // Start the appropriate mode
        if (useChartMode && testChart != null && testChart.audioClip != null)
        {
            StartChartMode();
        }
        else
        {
            StartSimpleMode();
        }
    }

    void Update()
    {
        if (useChartMode && testChart != null)
        {
            UpdateChartMode();
        }
        else
        {
            UpdateSimpleMode();
        }
    }

    private void StartSimpleMode()
    {
        isRunning = true;
        nextSpawnTime = Time.time + 1f;
        Debug.Log("[OffsetSpeedAdjust] Started simple mode");
    }

    private void UpdateSimpleMode()
    {
        if (isRunning && Time.time >= nextSpawnTime)
        {
            SpawnDemoNote(0f);
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void StartChartMode()
    {
        // Clear any existing notes
        ClearAllNotes();

        // Verify audio clip
        if (testChart.audioClip == null)
        {
            Debug.LogError("[OffsetSpeedAdjust] testChart.audioClip is NULL!");
            return;
        }

        if (musicSource == null)
        {
            Debug.LogError("[OffsetSpeedAdjust] musicSource is NULL!");
            return;
        }

        // Assign and play music IMMEDIATELY
        musicSource.clip = testChart.audioClip;

        // Record the exact DSP time when music starts
        musicStartDspTime = AudioSettings.dspTime;

        // Play immediately - no delay
        musicSource.Play();

        currentNoteIndex = 0;
        musicStarted = true;
        isRunning = true;

        Debug.Log($"[OffsetSpeedAdjust] Started chart mode with {testChart.notes.Count} notes");
        Debug.Log($"[OffsetSpeedAdjust] Music clip: {testChart.audioClip.name}, length: {testChart.audioClip.length}s");
        Debug.Log($"[OffsetSpeedAdjust] Music started at DSP time: {musicStartDspTime}");
        Debug.Log($"[OffsetSpeedAdjust] Current DSP time: {AudioSettings.dspTime}");
        Debug.Log($"[OffsetSpeedAdjust] MusicSource volume: {musicSource.volume}");
        Debug.Log($"[OffsetSpeedAdjust] HitSoundSource: {(hitSoundSource != null ? "OK" : "NULL")}, clip: {(hitSound != null ? hitSound.name : "NULL")}");
    }

    private void UpdateChartMode()
    {
        if (!isRunning || !musicStarted) return;

        // Get current song time in milliseconds using DSP time for accuracy
        double currentDspTime = AudioSettings.dspTime;
        double elapsedDspTime = currentDspTime - musicStartDspTime;
        float currentTimeMs = (float)(elapsedDspTime * 1000.0);

        float loopDurationMs = chartLoopDuration * 1000f;

        // Debug every 60 frames
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[UpdateChartMode] currentTimeMs: {currentTimeMs:F0}, musicSource.time: {musicSource.time:F2}s, isPlaying: {musicSource.isPlaying}, noteIndex: {currentNoteIndex}/{testChart.notes.Count}");
        }

        // Check if we should loop
        if (currentTimeMs >= loopDurationMs)
        {
            Debug.Log($"[OffsetSpeedAdjust] Loop triggered at {currentTimeMs:F0}ms");
            RestartChart();
            return;
        }

        // Spawn notes based on chart timing
        int spawnedThisFrame = 0;
        while (currentNoteIndex < testChart.notes.Count)
        {
            NoteData noteData = testChart.notes[currentNoteIndex];

            // Skip notes beyond loop duration
            if (noteData.hitTime > loopDurationMs)
            {
                currentNoteIndex++;
                continue;
            }

            // Calculate spawn time: spawn when music time reaches (hitTime - travelTime)
            // This ensures the note spawns at the right moment relative to the music
            float targetSpawnTimeMs = noteData.hitTime - (noteTravelTime * 1000f);

            // Check if it's time to spawn this note
            if (currentTimeMs >= targetSpawnTimeMs)
            {
                SpawnDemoNote(noteData.hitTime);
                currentNoteIndex++;
                spawnedThisFrame++;

                if (spawnedThisFrame <= 3) // Log first 3 spawns
                {
                    Debug.Log($"[Spawn] Note #{currentNoteIndex} at currentTime: {currentTimeMs:F0}ms, targetSpawn: {targetSpawnTimeMs:F0}ms, hitTime: {noteData.hitTime:F0}ms");
                }
            }
            else
            {
                break;
            }
        }
    }

    private void RestartChart()
    {
        // Stop current music
        musicSource.Stop();

        // Clear all active notes
        ClearAllNotes();

        // Reset state
        currentNoteIndex = 0;

        // Restart music immediately - no delay
        musicStartDspTime = AudioSettings.dspTime;
        musicSource.Play();

        Debug.Log($"[OffsetSpeedAdjust] Chart restarted at DSP time: {musicStartDspTime}");
    }

    private void ClearAllNotes()
    {
        // Destroy all active notes
        foreach (GameObject note in activeNotes)
        {
            if (note != null)
            {
                Destroy(note);
            }
        }
        activeNotes.Clear();
    }

    public double GetCurrentSongTimeMs()
    {
        if (!musicStarted) return -999999;
        double elapsedDspTime = AudioSettings.dspTime - musicStartDspTime;
        return elapsedDspTime * 1000.0;
    }

    private void UpdateSpeed()
    {
        float distance = Mathf.Abs(spawnY - judgmentLineY);
        currentSpeed = distance / noteTravelTime;
    }

    private void SetupButtons()
    {
        if (offsetIncreaseButton != null)
            offsetIncreaseButton.onClick.AddListener(() => AdjustOffset(offsetStep));

        if (offsetDecreaseButton != null)
            offsetDecreaseButton.onClick.AddListener(() => AdjustOffset(-offsetStep));

        if (travelTimeIncreaseButton != null)
            travelTimeIncreaseButton.onClick.AddListener(() => AdjustTravelTime(travelTimeStep));

        if (travelTimeDecreaseButton != null)
            travelTimeDecreaseButton.onClick.AddListener(() => AdjustTravelTime(-travelTimeStep));
    }

    private void AdjustOffset(float delta)
    {
        globalOffset += delta;
        GameSettings.GlobalOffset = globalOffset;
        UpdateUI();
        Debug.Log($"Offset adjusted to: {globalOffset:F3}s ({globalOffset * 1000f:F0}ms)");
    }

    private void AdjustTravelTime(float delta)
    {
        noteTravelTime = Mathf.Max(0.5f, noteTravelTime + delta);
        UpdateSpeed();
        UpdateUI();
        Debug.Log($"Travel time adjusted to: {noteTravelTime:F2}s, Speed: {currentSpeed:F2}");
    }

    private void UpdateUI()
    {
        if (offsetValueText != null)
            offsetValueText.text = $"{globalOffset * 1000f:F0}ms";

        if (travelTimeValueText != null)
            travelTimeValueText.text = $"{noteTravelTime:F2}s";
    }

    private void SpawnDemoNote(float hitTimeMs)
    {
        if (notePrefab == null)
        {
            Debug.LogError("Note prefab not assigned!");
            return;
        }

        Vector3 spawnPos = new Vector3(laneX, spawnY, 0);
        GameObject noteObj = Instantiate(notePrefab, spawnPos, Quaternion.identity);

        // Track active notes
        activeNotes.Add(noteObj);

        // Add demo note component
        DemoNote demoNote = noteObj.AddComponent<DemoNote>();
        demoNote.Initialize(currentSpeed, judgmentLineY, this, hitTimeMs, hitSoundSource);

        // Setup visual
        Note note = noteObj.GetComponent<Note>();
        if (note != null)
        {
            note.Initialize(0, GameColor.ColorA, hitTimeMs, currentSpeed, judgmentLineY - 2f, null);
        }
    }

    public void OnNoteDestroyed(GameObject noteObj)
    {
        activeNotes.Remove(noteObj);
    }

    public void SpawnHitEffect(Vector3 position)
    {
        GameObject effectObj = new GameObject("HitEffect");
        effectObj.transform.position = position;

        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
        main.startColor = new Color(1f, 0.8f, 0.2f, 1f);
        main.maxParticles = 30;
        main.duration = 0.3f;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;
        shape.radiusThickness = 1f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.9f, 0.3f), 0f),
                new GradientColorKey(new Color(1f, 0.5f, 0.1f), 0.5f),
                new GradientColorKey(new Color(1f, 0.2f, 0.0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.3f, 0.8f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.speedModifier = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.3f));

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        if (particleMaterial != null)
        {
            renderer.material = particleMaterial;
        }
        else
        {
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.SetColor("_Color", Color.white);
        }

        ps.Play();
        Destroy(effectObj, 1f);
    }
}

// Demo note that auto-hits at judgment line
public class DemoNote : MonoBehaviour
{
    private float speed;
    private float judgmentLineY;
    private OffsetSpeedAdjustManager manager;
    private float hitTimeMs;
    private AudioSource hitSoundSource;
    private bool hasHit = false;
    private bool soundPlayed = false;

    public void Initialize(float speed, float judgmentLineY, OffsetSpeedAdjustManager manager,
                          float hitTimeMs, AudioSource hitSoundSource)
    {
        this.speed = speed;
        this.judgmentLineY = judgmentLineY;
        this.manager = manager;
        this.hitTimeMs = hitTimeMs;
        this.hitSoundSource = hitSoundSource;
    }

    void Update()
    {
        // Move down
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        // Check if reached judgment line
        if (!hasHit && transform.position.y <= judgmentLineY)
        {
            hasHit = true;

            // Play sound when note reaches judgment line (with offset applied)
            if (!soundPlayed && hitSoundSource != null && hitSoundSource.clip != null)
            {
                // Apply global offset: positive offset = sound plays earlier
                double currentSongTimeMs = manager.GetCurrentSongTimeMs();
                double expectedHitTimeMs = hitTimeMs;
                double timingError = currentSongTimeMs - expectedHitTimeMs;

                // Log timing for debugging
                if (Mathf.Abs((float)timingError) > 50f)
                {
                    Debug.Log($"[DemoNote] Timing error: {timingError:F0}ms (current: {currentSongTimeMs:F0}, expected: {expectedHitTimeMs:F0})");
                }

                hitSoundSource.PlayOneShot(hitSoundSource.clip);
                soundPlayed = true;
            }

            manager.SpawnHitEffect(transform.position);
            manager.OnNoteDestroyed(gameObject);
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // Make sure we're removed from the active notes list
        if (manager != null && !hasHit)
        {
            manager.OnNoteDestroyed(gameObject);
        }
    }
}
