using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimelinePanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TimelineSystem timelineSystem;
    [SerializeField] RectTransform playhead;
    [SerializeField] RectTransform playheadArea;
    [SerializeField] RectTransform trackRowsContainer;
    [SerializeField] TrackRowUI trackRowPrefab;
    [SerializeField] Button playButton;
    [SerializeField] Button pauseButton;
    [SerializeField] Button resetButton;

    readonly List<TrackRowUI> trackRows = new List<TrackRowUI>();

    void Awake()
    {
        if (timelineSystem == null)
        {
            timelineSystem = TimelineSystem.Instance;
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OnPauseClicked);
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetClicked);
        }
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
            timelineSystem.TracksChanged += RebuildTrackRows;
            timelineSystem.PlayStateChanged += HandlePlayStateChanged;
        }

        RebuildTrackRows();
        RefreshPlayhead(timelineSystem != null ? timelineSystem.NormalizedTime : 0f);
        RefreshButtons(timelineSystem != null && timelineSystem.IsPlaying);
    }

    void OnDisable()
    {
        if (timelineSystem != null)
        {
            timelineSystem.TimeChanged -= HandleTimeChanged;
            timelineSystem.TracksChanged -= RebuildTrackRows;
            timelineSystem.PlayStateChanged -= HandlePlayStateChanged;
        }
    }

    void OnPlayClicked()
    {
        timelineSystem?.Play();
    }

    void OnPauseClicked()
    {
        timelineSystem?.Pause();
    }

    void OnResetClicked()
    {
        timelineSystem?.ResetTimeline();
        RebuildTrackRows();
    }

    void HandleTimeChanged(float normalizedTime)
    {
        RefreshPlayhead(normalizedTime);
    }

    void HandlePlayStateChanged(bool isPlaying)
    {
        RefreshButtons(isPlaying);
    }

    void RefreshButtons(bool isPlaying)
    {
        if (playButton != null)
        {
            playButton.interactable = !isPlaying;
        }

        if (pauseButton != null)
        {
            pauseButton.interactable = isPlaying;
        }
    }

    void RefreshPlayhead(float normalizedTime)
    {
        if (playhead == null || playheadArea == null)
        {
            return;
        }

        float x = normalizedTime * playheadArea.rect.width;
        Vector2 anchoredPosition = playhead.anchoredPosition;
        anchoredPosition.x = x;
        playhead.anchoredPosition = anchoredPosition;
    }

    void RebuildTrackRows()
    {
        ClearTrackRows();

        if (timelineSystem == null || trackRowPrefab == null || trackRowsContainer == null)
        {
            return;
        }

        IReadOnlyList<TimelineTrack> tracks = timelineSystem.Tracks;
        for (int i = 0; i < tracks.Count; i++)
        {
            TrackRowUI row = Instantiate(trackRowPrefab, trackRowsContainer);
            row.Bind(tracks[i]);
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
}
