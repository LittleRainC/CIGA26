using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
public class TimelinePanel : MonoBehaviour
{
    const float TrackRowHeight = 40f;

    [Header("Timeline Layout")]
    [SerializeField] float labelColumnWidth = 120f;
    [SerializeField] float timelineContentWidth = 550f;
    [Tooltip("Label 文字相对行左缘的内边距。不要设负数，否则文字会跑出屏幕。")]
    [SerializeField] float labelOffsetX = 4f;
    [Tooltip("PlayheadArea 在 TimelinePanel 内的 Y 位置，保存在场景里可团队同步。")]
    [SerializeField] float playheadAreaY = 35f;

    [Header("References")]
    [SerializeField] TimelineSystem timelineSystem;
    [SerializeField] RectTransform playhead;
    [SerializeField] RectTransform playheadArea;
    [SerializeField] RectTransform trackRowsContainer;
    [SerializeField] TrackRowUI trackRowPrefab;
    [SerializeField] DraggableKeyframe keyframeHandlePrefab;
    [SerializeField] RectTransform gridLinePrefab;
    [SerializeField] TextMeshProUGUI timeLabel;
    [SerializeField] Button resetButton;
    [SerializeField] TimelineRulerUI timelineRuler;

    readonly List<TrackRowUI> trackRows = new List<TrackRowUI>();

    void Awake()
    {
        if (timelineSystem == null)
        {
            timelineSystem = TimelineSystem.Instance;
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetClicked);
        }

        EnsureTrackRowsLayout();
        ApplySharedLayout();
    }

    void ApplySharedLayout()
    {
        if (playheadArea == null)
        {
            return;
        }

        playheadArea.pivot = new Vector2(0f, 0.5f);
        playheadArea.anchorMin = new Vector2(0f, 0.5f);
        playheadArea.anchorMax = new Vector2(0f, 0.5f);
        playheadArea.anchoredPosition = new Vector2(labelColumnWidth, playheadAreaY);
        playheadArea.sizeDelta = new Vector2(timelineContentWidth, playheadArea.sizeDelta.y);
    }

    TimelineLayoutSettings GetLayoutSettings()
    {
        return new TimelineLayoutSettings(labelColumnWidth, timelineContentWidth, labelOffsetX);
    }

    void EnsureTrackRowsLayout()
    {
        if (trackRowsContainer == null)
        {
            return;
        }

        VerticalLayoutGroup layout = trackRowsContainer.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = trackRowsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        layout.spacing = 8f;
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
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
        }

        ApplySharedLayout();
        RebuildTrackRows();
        RefreshPlayhead(timelineSystem != null ? timelineSystem.CurrentTime : 0f);
    }

    void Start()
    {
        ApplySharedLayout();
        RebuildTrackRows();
    }

    void OnDisable()
    {
        if (timelineSystem != null)
        {
            timelineSystem.TimeChanged -= HandleTimeChanged;
            timelineSystem.TracksChanged -= RebuildTrackRows;
        }
    }

    void OnResetClicked()
    {
        timelineSystem?.ResetTimeline();
    }

    void HandleTimeChanged(float currentTime)
    {
        RefreshPlayhead(currentTime);
    }

    void RefreshPlayhead(float currentTime)
    {
        if (playhead == null || playheadArea == null || timelineSystem == null)
        {
            return;
        }

        playhead.pivot = new Vector2(0f, 0.5f);
        playhead.anchorMin = new Vector2(0f, 0.5f);
        playhead.anchorMax = new Vector2(0f, 0.5f);

        float normalized = timelineSystem.TimeToNormalized(currentTime);
        playhead.anchoredPosition = new Vector2(normalized * playheadArea.rect.width, playhead.anchoredPosition.y);
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

        ApplySharedLayout();
        Canvas.ForceUpdateCanvases();

        IReadOnlyList<TimelineTrack> tracks = timelineSystem.Tracks;
        for (int i = 0; i < tracks.Count; i++)
        {
            TrackRowUI row = Instantiate(trackRowPrefab, trackRowsContainer);
            SetupTrackRowLayout(row.transform as RectTransform);
            row.Bind(
                tracks[i],
                timelineSystem,
                GetLayoutSettings(),
                keyframeHandlePrefab,
                gridLinePrefab);
            trackRows.Add(row);
        }

        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < trackRows.Count; i++)
        {
            trackRows[i].RefreshLayout();
        }

        timelineRuler?.Rebuild();
    }

    static void SetupTrackRowLayout(RectTransform rowRect)
    {
        if (rowRect == null)
        {
            return;
        }

        rowRect.localScale = Vector3.one;
        rowRect.pivot = new Vector2(0f, 1f);
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.anchoredPosition = Vector2.zero;
        rowRect.sizeDelta = new Vector2(0f, TrackRowHeight);
        rowRect.offsetMin = new Vector2(0f, rowRect.offsetMin.y);
        rowRect.offsetMax = new Vector2(0f, rowRect.offsetMax.y);
    }

    void ClearTrackRows()
    {
        for (int i = 0; i < trackRows.Count; i++)
        {
            if (trackRows[i] != null)
            {
                trackRows[i].Unbind();
                Destroy(trackRows[i].gameObject);
            }
        }

        trackRows.Clear();
    }
}
