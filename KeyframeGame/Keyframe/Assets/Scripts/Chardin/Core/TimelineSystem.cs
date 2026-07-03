using System;
using System.Collections.Generic;
using UnityEngine;

public class TimelineSystem : MonoBehaviour
{
    public static TimelineSystem Instance { get; private set; }

    [Header("Playback")]
    [SerializeField] float playSpeed = 0.12f;
    [SerializeField] bool loopPlayback;

    [Header("Debug")]
    [SerializeField] bool allowDebugScrub = true;
    [SerializeField, Range(0f, 1f)] float debugNormalizedTime;

    readonly List<TimelineTrack> tracks = new List<TimelineTrack>();

    public float NormalizedTime { get; private set; }
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

    void Update()
    {
        if (IsPlaying)
        {
            SetTime(NormalizedTime + Time.deltaTime * playSpeed);
            if (NormalizedTime >= 1f)
            {
                if (loopPlayback)
                {
                    SetTime(0f);
                }
                else
                {
                    SetTime(1f);
                    Pause();
                }
            }

            return;
        }

        if (allowDebugScrub && !Mathf.Approximately(debugNormalizedTime, NormalizedTime))
        {
            SetTime(debugNormalizedTime);
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
        debugNormalizedTime = NormalizedTime;
        PlayStateChanged?.Invoke(false);
    }

    public void ResetTimeline()
    {
        Pause();
        SetTime(0f);
    }

    public void SetTime(float normalizedTime)
    {
        NormalizedTime = Mathf.Clamp01(normalizedTime);
        debugNormalizedTime = NormalizedTime;
        EvaluateAllTracks();
        TimeChanged?.Invoke(NormalizedTime);
    }

    public void EvaluateAllTracks()
    {
        for (int i = 0; i < tracks.Count; i++)
        {
            tracks[i].Evaluate(NormalizedTime);
        }
    }

    public void NotifyTrackAnchorsChanged()
    {
        EvaluateAllTracks();
    }
}
