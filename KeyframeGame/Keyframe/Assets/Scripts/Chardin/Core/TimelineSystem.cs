using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class TimelineSystem : MonoBehaviour
{
    public static TimelineSystem Instance { get; private set; }

    [Header("Timeline")]
    [SerializeField] float duration = 30f;
    [SerializeField] float gridInterval = 5f;

    [Header("Input")]
    [SerializeField] KeyCode playKey = KeyCode.Space;

    readonly List<TimelineTrack> tracks = new List<TimelineTrack>();

    public float Duration => duration;
    public float GridInterval => gridInterval;
    public float CurrentTime { get; private set; }
    public bool IsPlaying { get; private set; }
    public IReadOnlyList<TimelineTrack> Tracks => tracks;

    public event Action<float> TimeChanged;
    public event Action TracksChanged;
    public event Action TimelineReset;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple TimelineSystem instances found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        ResetTimeline();
    }

    void Update()
    {
        if (Input.GetKeyDown(playKey) && !IsPlaying)
        {
            Play();
        }

        if (!IsPlaying)
        {
            return;
        }

        SetTime(CurrentTime + Time.deltaTime);
        if (CurrentTime >= duration)
        {
            FinishPlayback();
        }
    }

    public void RegisterTrack(TimelineTrack track)
    {
        if (track == null || tracks.Contains(track))
        {
            return;
        }

        tracks.Add(track);
        tracks.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal));
        EvaluateAllTracks();
        TracksChanged?.Invoke();
    }

    public void UnregisterTrack(TimelineTrack track)
    {
        if (track == null || !tracks.Remove(track))
        {
            return;
        }

        TracksChanged?.Invoke();
    }

    public void Play()
    {
        if (IsPlaying)
        {
            return;
        }

        IsPlaying = true;
    }

    public void Pause()
    {
        IsPlaying = false;
    }

    public void ResetTimeline()
    {
        Pause();
        SetTime(0f);
        TimelineReset?.Invoke();
    }

    public void SetTime(float timeInSeconds)
    {
        CurrentTime = Mathf.Clamp(timeInSeconds, 0f, duration);
        EvaluateAllTracks();
        TimeChanged?.Invoke(CurrentTime);
    }

    public void EvaluateAllTracks()
    {
        for (int i = 0; i < tracks.Count; i++)
        {
            tracks[i].Evaluate(CurrentTime);
        }
    }

    public void OnKeyframesEdited()
    {
        if (!IsPlaying)
        {
            EvaluateAllTracks();
        }
    }

    public float TimeToNormalized(float timeInSeconds)
    {
        return duration <= Mathf.Epsilon ? 0f : Mathf.Clamp01(timeInSeconds / duration);
    }

    public float NormalizedToTime(float normalized)
    {
        return SnapTime(Mathf.Clamp01(normalized) * duration);
    }

    public float SnapTime(float timeInSeconds)
    {
        timeInSeconds = Mathf.Clamp(timeInSeconds, 0f, duration);

        if (gridInterval <= Mathf.Epsilon)
        {
            return timeInSeconds;
        }

        float snapped = Mathf.Round(timeInSeconds / gridInterval) * gridInterval;
        return Mathf.Clamp(snapped, 0f, duration);
    }

    void FinishPlayback()
    {
        Pause();
        SetTime(0f);
        TimelineReset?.Invoke();
    }
}
