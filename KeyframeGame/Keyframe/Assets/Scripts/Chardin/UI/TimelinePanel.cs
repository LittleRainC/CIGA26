using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimelinePanel : MonoBehaviour
{
    public static TimelinePanel Instance { get; private set; }

    [Header("References")]
    [SerializeField] TimelineSystem timelineSystem;
    [SerializeField] RectTransform playhead;
    [SerializeField] RectTransform playheadArea;
    [SerializeField] RectTransform trackRowsContainer;
    [SerializeField] RectTransform trackDropZone;
    [SerializeField] TrackRowUI trackRowPrefab;
    [SerializeField] Text timeLabel;
    [SerializeField] Button resetButton;
    [SerializeField] GameObject emptyHint;

    [Header("Optional Preload")]
    [SerializeField] List<TimelineTrack> initialDisplayedTracks = new List<TimelineTrack>();

    readonly List<TimelineTrack> displayedTracks = new List<TimelineTrack>();
    readonly List<TrackRowUI> trackRows = new List<TrackRowUI>();

    Canvas rootCanvas;

    public IReadOnlyList<TimelineTrack> DisplayedTracks => displayedTracks;

    public event Action DisplayedTracksChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple TimelinePanel instances found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        rootCanvas = GetComponentInParent<Canvas>();

        if (timelineSystem == null)
        {
            timelineSystem = TimelineSystem.Instance;
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetClicked);
        }

        if (trackDropZone == null)
        {
            trackDropZone = trackRowsContainer;
        }
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
        for (int i = 0; i < initialDisplayedTracks.Count; i++)
        {
            TryAddDisplayedTrack(initialDisplayedTracks[i]);
        }

        RefreshEmptyHint();
    }

    void OnEnable()
    {
        if (timelineSystem == null)
        {
            timelineSystem = TimelineSystem.Instance;
        }

        if (timelineSystem != null)
        {
            timelineSystem.TimeChanged += HandleTimeChanged;
            timelineSystem.PlayStateChanged += HandlePlayStateChanged;
        }

        RefreshPlayhead(timelineSystem != null ? timelineSystem.CurrentTime : 0f);
    }

    void OnDisable()
    {
        if (timelineSystem != null)
        {
            timelineSystem.TimeChanged -= HandleTimeChanged;
            timelineSystem.PlayStateChanged -= HandlePlayStateChanged;
        }
    }

    public bool IsTrackDisplayed(TimelineTrack track)
    {
        return track != null && displayedTracks.Contains(track);
    }

    public bool TryAddDisplayedTrack(TimelineTrack track)
    {
        if (track == null || displayedTracks.Contains(track))
        {
            return false;
        }

        displayedTracks.Add(track);
        RebuildTrackRows();
        RefreshEmptyHint();
        DisplayedTracksChanged?.Invoke();
        return true;
    }

    public void RemoveDisplayedTrack(TimelineTrack track)
    {
        if (track == null || !displayedTracks.Remove(track))
        {
            return;
        }

        RebuildTrackRows();
        RefreshEmptyHint();
        DisplayedTracksChanged?.Invoke();
    }

    public bool IsPointerOverDropZone(Vector2 screenPosition)
    {
        if (trackDropZone == null)
        {
            return false;
        }

        Camera eventCamera = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? rootCanvas.worldCamera
            : null;

        return RectTransformUtility.RectangleContainsScreenPoint(trackDropZone, screenPosition, eventCamera);
    }

    public void HandleTrackDrop(TimelineTrack track)
    {
        TryAddDisplayedTrack(track);
    }

    void OnResetClicked()
    {
        timelineSystem?.ResetTimeline();
    }

    void HandleTimeChanged(float currentTime)
    {
        RefreshPlayhead(currentTime);
    }

    void HandlePlayStateChanged(bool isPlaying)
    {
        RefreshTimeLabel(timelineSystem != null ? timelineSystem.CurrentTime : 0f);
    }

    void RefreshPlayhead(float currentTime)
    {
        if (playhead == null || playheadArea == null || timelineSystem == null)
        {
            return;
        }

        float normalized = timelineSystem.TimeToNormalized(currentTime);
        Vector2 anchoredPosition = playhead.anchoredPosition;
        anchoredPosition.x = normalized * playheadArea.rect.width;
        playhead.anchoredPosition = anchoredPosition;
        RefreshTimeLabel(currentTime);
    }

    void RefreshTimeLabel(float currentTime)
    {
        if (timeLabel == null || timelineSystem == null)
        {
            return;
        }

        timeLabel.text = $"{currentTime:0.0}s / {timelineSystem.Duration:0.0}s";
    }

    void RebuildTrackRows()
    {
        ClearTrackRows();

        if (timelineSystem == null || trackRowPrefab == null || trackRowsContainer == null)
        {
            return;
        }

        for (int i = 0; i < displayedTracks.Count; i++)
        {
            TrackRowUI row = Instantiate(trackRowPrefab, trackRowsContainer);
            row.Bind(displayedTracks[i], timelineSystem);
            trackRows.Add(row);
        }
    }

    void ClearTrackRows()
    {
        for (int i = 0; i < trackRows.Count; i++)
        {
            if (trackRows[i] != null)
            {
                Destroy(trackRows[i].gameObject);
            }
        }

        trackRows.Clear();
    }

    void RefreshEmptyHint()
    {
        if (emptyHint != null)
        {
            emptyHint.SetActive(displayedTracks.Count == 0);
        }
    }
}
