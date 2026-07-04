using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
[ExecuteAlways]
public class TimelinePanel : MonoBehaviour
{
    [Header("Panel")]
    [Tooltip("TimelinePanel 贴屏幕底边的高度。")]
    [SerializeField] float panelHeight = 200f;

    [Header("Timeline Layout")]
    [SerializeField] float labelColumnWidth = 120f;
    [SerializeField] float timelineContentWidth = 550f;
    [Tooltip("Label 文字相对行左缘的内边距。不要设负数。")]
    [SerializeField] float labelOffsetX = 4f;
    [Tooltip("PlayheadArea 底边距 Panel 底边的距离。")]
    [SerializeField] float playheadAreaY = 70f;
    [Tooltip("TrackRows 底边距 Panel 底边的距离。")]
    [SerializeField] float trackRowsY = 20f;
    [Tooltip("TrackRows 容器高度。")]
    [SerializeField] float trackRowsHeight = 58f;
    [Tooltip("每条 TrackRow 的高度。")]
    [SerializeField] float trackRowHeight = 40f;
    [Tooltip("TrackRow 之间的间距。")]
    [SerializeField] float trackRowSpacing = 8f;
    [Tooltip("勾选后时间轴内容在 Panel 内水平居中。")]
    [SerializeField] bool centerTimelineContent = true;

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

        if (Application.isPlaying && resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetClicked);
        }

        ApplyPanelLayout();
    }

    void OnValidate()
    {
        labelOffsetX = Mathf.Max(0f, labelOffsetX);
        trackRowHeight = Mathf.Max(40f, trackRowHeight);
        ApplyPanelLayout();
    }

    void ApplyPanelLayout()
    {
        ApplyPanelSelfLayout();
        ApplyHeaderLayout();
        EnsureTrackRowsLayout();
        ApplyPlayheadAreaLayout();
        ApplyTrackRowsLayout();
    }

    void ApplyPanelSelfLayout()
    {
        RectTransform panel = transform as RectTransform;
        if (panel == null)
        {
            return;
        }

        panel.pivot = new Vector2(0.5f, 0f);
        panel.anchorMin = new Vector2(0f, 0f);
        panel.anchorMax = new Vector2(1f, 0f);
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = new Vector2(0f, panelHeight);
    }

    float GetTimelineContentLeft()
    {
        if (!centerTimelineContent)
        {
            return labelColumnWidth;
        }

        RectTransform panel = transform as RectTransform;
        if (panel == null)
        {
            return labelColumnWidth;
        }

        return Mathf.Max(0f, (panel.rect.width - timelineContentWidth) * 0.5f);
    }

    float GetTrackRowsLeft()
    {
        if (!centerTimelineContent)
        {
            return 0f;
        }

        return GetTimelineContentLeft() - labelColumnWidth;
    }

    void ApplyHeaderLayout()
    {
        if (timeLabel != null)
        {
            RectTransform labelRect = timeLabel.rectTransform;
            labelRect.pivot = new Vector2(0f, 1f);
            labelRect.anchorMin = new Vector2(0f, 1f);
            labelRect.anchorMax = new Vector2(0f, 1f);
            labelRect.anchoredPosition = new Vector2(8f, -8f);
        }

        if (resetButton != null)
        {
            RectTransform buttonRect = resetButton.transform as RectTransform;
            buttonRect.pivot = new Vector2(1f, 1f);
            buttonRect.anchorMin = new Vector2(1f, 1f);
            buttonRect.anchorMax = new Vector2(1f, 1f);
            buttonRect.anchoredPosition = new Vector2(-8f, -8f);
        }
    }

    void ApplyPlayheadAreaLayout()
    {
        if (playheadArea == null)
        {
            return;
        }

        playheadArea.pivot = new Vector2(0f, 0f);
        playheadArea.anchorMin = new Vector2(0f, 0f);
        playheadArea.anchorMax = new Vector2(0f, 0f);
        playheadArea.anchoredPosition = new Vector2(GetTimelineContentLeft(), playheadAreaY);
        playheadArea.sizeDelta = new Vector2(timelineContentWidth, playheadArea.sizeDelta.y);
    }

    void ApplyTrackRowsLayout()
    {
        if (trackRowsContainer == null)
        {
            return;
        }

        float totalWidth = labelColumnWidth + timelineContentWidth;

        if (centerTimelineContent)
        {
            trackRowsContainer.pivot = new Vector2(0f, 0f);
            trackRowsContainer.anchorMin = new Vector2(0f, 0f);
            trackRowsContainer.anchorMax = new Vector2(0f, 0f);
            trackRowsContainer.anchoredPosition = new Vector2(GetTrackRowsLeft(), trackRowsY);
            trackRowsContainer.sizeDelta = new Vector2(totalWidth, trackRowsHeight);
        }
        else
        {
            trackRowsContainer.pivot = new Vector2(0.5f, 0f);
            trackRowsContainer.anchorMin = new Vector2(0f, 0f);
            trackRowsContainer.anchorMax = new Vector2(1f, 0f);
            trackRowsContainer.anchoredPosition = new Vector2(0f, trackRowsY);
            trackRowsContainer.sizeDelta = new Vector2(0f, trackRowsHeight);
        }
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

        layout.spacing = trackRowSpacing;
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

        if (Application.isPlaying && timelineSystem != null)
        {
            timelineSystem.TimeChanged += HandleTimeChanged;
            timelineSystem.TracksChanged += RebuildTrackRows;
        }

        ApplyPanelLayout();

        if (!Application.isPlaying)
        {
            return;
        }

        RebuildTrackRows();
        RefreshPlayhead(timelineSystem != null ? timelineSystem.CurrentTime : 0f);
    }

    void Start()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        ApplyPanelLayout();
        RebuildTrackRows();
    }

    void OnDisable()
    {
        if (!Application.isPlaying || timelineSystem == null)
        {
            return;
        }

        timelineSystem.TimeChanged -= HandleTimeChanged;
        timelineSystem.TracksChanged -= RebuildTrackRows;
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

        ApplyPanelLayout();
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

    void SetupTrackRowLayout(RectTransform rowRect)
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
        rowRect.sizeDelta = new Vector2(0f, Mathf.Max(40f, trackRowHeight));
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
                DestroyImmediateSafe(trackRows[i].gameObject);
            }
        }

        trackRows.Clear();
    }

    static void DestroyImmediateSafe(Object target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }
}
