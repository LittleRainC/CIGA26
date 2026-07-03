using System;
using System.Collections.Generic;
using UnityEngine;

public class TimelineSystem : MonoBehaviour
{
    public static TimelineSystem Instance { get; private set; }

    [Header("Timeline")]
    [SerializeField] float duration = 10f;

    [Header("Input")]
    [SerializeField] KeyCode playKey = KeyCode.Space;

    [Header("Debug")]
    [SerializeField] bool allowDebugScrub = true;
    [SerializeField] float debugCurrentTime;

    readonly List<TimelineTrack> tracks = new List<TimelineTrack>();

    public float Duration => duration;
    public float CurrentTime { get; private set; }
    public float NormalizedTime => duration <= Mathf.Epsilon ? 0f : Mathf.Clamp01(CurrentTime / duration);
    public bool IsPlaying { get; private set; }
    public IReadOnlyList<TimelineTrack> Tracks => tracks;

    public event Action<float> TimeChanged;
    public event Action<bool> PlayStateChanged;
    public event Action TracksChanged;

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

        if (IsPlaying)
        {
            SetTime(CurrentTime + Time.deltaTime);
            if (CurrentTime >= duration)
            {
                FinishPlayback();
            }

            return;
        }

        if (allowDebugScrub && !Mathf.Approximately(debugCurrentTime, CurrentTime))
        {
            SetTime(debugCurrentTime);
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
        PlayStateChanged?.Invoke(true);
    }

    public void Pause()
    {
        if (!IsPlaying)
        {
            return;
        }

        IsPlaying = false;
        PlayStateChanged?.Invoke(false);
    }

    public void ResetTimeline()
    {
        Pause();
        SetTime(0f);
    }

    public void SetTime(float timeInSeconds)
    {
        CurrentTime = Mathf.Clamp(timeInSeconds, 0f, duration);
        debugCurrentTime = CurrentTime;
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
        return Mathf.Clamp01(normalized) * duration;
    }

    void FinishPlayback()
    {
        Pause();
        SetTime(0f);
    }
}
