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
    [Tooltip("Label 文字相对默认位置的左右偏移。负值 = 往左，正值 = 往右。不影响轨道条对齐。")]
    [SerializeField] float labelOffsetX = -15f;
    [Tooltip("开启后 TrackRows 宽度会自动跟 TimelinePanel 对齐；关闭后可手动调整 TrackRows 的位置和大小。")]
    [SerializeField] bool autoSyncTrackRowsLayout = true;

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
        SyncTrackRowsContainerLayout();
        ApplyTimelineLayout();
    }

    void SyncTrackRowsContainerLayout()
    {
        if (!autoSyncTrackRowsLayout || trackRowsContainer == null)
        {
            return;
        }

        RectTransform panelRect = transform as RectTransform;
        RectTransform containerParent = trackRowsContainer.parent as RectTransform;
        if (panelRect == null || containerParent == null)
        {
            return;
        }

        Vector3[] panelCorners = new Vector3[4];
        panelRect.GetWorldCorners(panelCorners);

        Vector2 localLeft = containerParent.InverseTransformPoint(panelCorners[0]);
        Vector2 localRight = containerParent.InverseTransformPoint(panelCorners[3]);

        float width = localRight.x - localLeft.x;
        float centerX = (localLeft.x + localRight.x) * 0.5f;

        trackRowsContainer.anchorMin = new Vector2(0.5f, trackRowsContainer.anchorMin.y);
        trackRowsContainer.anchorMax = new Vector2(0.5f, trackRowsContainer.anchorMax.y);
        trackRowsContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        trackRowsContainer.anchoredPosition = new Vector2(centerX, trackRowsContainer.anchoredPosition.y);
    }

    void ApplyTimelineLayout()
    {
        if (playheadArea == null)
        {
            return;
        }

        playheadArea.pivot = new Vector2(0f, 0.5f);
        playheadArea.anchorMin = new Vector2(0f, 0.5f);
        playheadArea.anchorMax = new Vector2(0f, 0.5f);
        playheadArea.anchoredPosition = new Vector2(labelColumnWidth, playheadArea.anchoredPosition.y);
        playheadArea.sizeDelta = new Vector2(timelineContentWidth, playheadArea.sizeDelta.y);
        SyncTrackRowsContainerLayout();
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

        if (trackRowsContainer.GetComponent<VerticalLayoutGroup>() == null)
        {
            VerticalLayoutGroup layout = trackRowsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
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
        }

        RebuildTrackRows();
        ApplyTimelineLayout();
        RefreshPlayhead(timelineSystem != null ? timelineSystem.CurrentTime : 0f);
    }

    void Start()
    {
        ApplyTimelineLayout();
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

        ApplyTimelineLayout();
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
                playheadArea,
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
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
        rowRect.anchoredPosition = Vector2.zero;
        rowRect.sizeDelta = new Vector2(0f, TrackRowHeight);
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
