using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrackRowUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI labelText;
    [SerializeField] RectTransform trackBar;
    [SerializeField] DraggableKeyframe keyframeHandlePrefab;
    [SerializeField] RectTransform gridLinePrefab;

    readonly Dictionary<string, DraggableKeyframe> handles = new Dictionary<string, DraggableKeyframe>();
    readonly List<RectTransform> gridLines = new List<RectTransform>();

    TimelineTrack track;
    TimelineSystem timelineSystem;
    TimelineLayoutSettings layout;
    DraggableKeyframe handlePrefab;
    RectTransform linePrefab;

    public TimelineTrack Track => track;

    public void Bind(
        TimelineTrack boundTrack,
        TimelineSystem system,
        TimelineLayoutSettings layoutSettings,
        DraggableKeyframe handlePrefabOverride,
        RectTransform gridLinePrefabOverride)
    {
        layout = layoutSettings;
        handlePrefab = handlePrefabOverride != null ? handlePrefabOverride : keyframeHandlePrefab;
        linePrefab = gridLinePrefabOverride != null ? gridLinePrefabOverride : gridLinePrefab;

        if (track != null)
        {
            track.KeyframesChanged -= RebuildHandles;
        }

        track = boundTrack;
        timelineSystem = system;

        if (track != null)
        {
            track.KeyframesChanged += RebuildHandles;
        }

        if (labelText != null && track != null)
        {
            labelText.text = track.DisplayName;
        }

        ApplyLayoutSettings();
        if (trackBar != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(trackBar);
        }

        RebuildGridLines();
        RebuildHandles();
    }

    public void RefreshLayout()
    {
        ApplyLayoutSettings();
        LayoutRebuilder.ForceRebuildLayoutImmediate(trackBar);
        RebuildGridLines();
        RefreshHandlePositions();
    }

    public void Unbind()
    {
        if (track != null)
        {
            track.KeyframesChanged -= RebuildHandles;
        }

        track = null;
        timelineSystem = null;
        ClearHandles();
        ClearGridLines();
    }

    void ApplyLayoutSettings()
    {
        SetupLabelLayout();
        SetupTrackBarLayout();
    }

    void SetupLabelLayout()
    {
        if (labelText == null)
        {
            return;
        }

        RectTransform labelRect = labelText.rectTransform;
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(0f, 0.5f);

        float labelPadding = Mathf.Max(0f, layout.labelOffsetX);
        labelRect.anchoredPosition = new Vector2(labelPadding, 0f);
        labelRect.sizeDelta = new Vector2(layout.labelColumnWidth, labelRect.sizeDelta.y);

        labelText.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        labelText.overflowMode = TMPro.TextOverflowModes.Truncate;
    }

    void SetupTrackBarLayout()
    {
        if (trackBar == null)
        {
            return;
        }

        trackBar.pivot = new Vector2(0f, 0.5f);
        trackBar.anchorMin = new Vector2(0f, 0.5f);
        trackBar.anchorMax = new Vector2(0f, 0.5f);
        trackBar.anchoredPosition = new Vector2(layout.labelColumnWidth, trackBar.anchoredPosition.y);
        trackBar.sizeDelta = new Vector2(layout.timelineContentWidth, trackBar.sizeDelta.y);
    }

    void OnDestroy()
    {
        if (track != null)
        {
            track.KeyframesChanged -= RebuildHandles;
        }
    }

    public void RebuildHandles()
    {
        ClearHandles();

        if (track == null || trackBar == null)
        {
            return;
        }

        if (handlePrefab == null)
        {
            Debug.LogWarning($"TrackRowUI on '{name}' is missing Keyframe Handle Prefab.", this);
            return;
        }

        IReadOnlyList<TimelineKeyframe> keyframes = track.Keyframes;
        for (int i = 0; i < keyframes.Count; i++)
        {
            TimelineKeyframe keyframe = keyframes[i];
            DraggableKeyframe handle = Instantiate(handlePrefab, trackBar);
            handle.gameObject.SetActive(true);
            handle.Initialize(this, keyframe.Id, trackBar);
            handles[keyframe.Id] = handle;
            SetHandlePosition(handle.GetComponent<RectTransform>(), keyframe.Time);
        }
    }

    void RefreshHandlePositions()
    {
        if (track == null || trackBar == null || timelineSystem == null)
        {
            return;
        }

        if (handles.Count != track.Keyframes.Count)
        {
            RebuildHandles();
            return;
        }

        IReadOnlyList<TimelineKeyframe> keyframes = track.Keyframes;
        for (int i = 0; i < keyframes.Count; i++)
        {
            TimelineKeyframe keyframe = keyframes[i];
            if (handles.TryGetValue(keyframe.Id, out DraggableKeyframe handle))
            {
                SetHandlePosition(handle.GetComponent<RectTransform>(), keyframe.Time);
            }
            else
            {
                RebuildHandles();
                return;
            }
        }
    }

    float GetTrackBarWidth()
    {
        if (trackBar == null)
        {
            return layout.timelineContentWidth;
        }

        float width = trackBar.rect.width;
        if (width <= Mathf.Epsilon)
        {
            width = layout.timelineContentWidth;
        }

        return width;
    }

    public void UpdateHandlePreview(string keyframeId, float timeInSeconds)
    {
        if (handles.TryGetValue(keyframeId, out DraggableKeyframe handle))
        {
            SetHandlePosition(handle.GetComponent<RectTransform>(), timeInSeconds);
        }
    }

    public void CommitKeyframeTime(string keyframeId, float timeInSeconds)
    {
        if (track == null)
        {
            return;
        }

        track.SetKeyframeTime(keyframeId, timeInSeconds);
        timelineSystem?.OnKeyframesEdited();
        RebuildHandles();
    }

    void RebuildGridLines()
    {
        ClearGridLines();

        if (timelineSystem == null || trackBar == null || linePrefab == null)
        {
            return;
        }

        float interval = timelineSystem.GridInterval;
        if (interval <= Mathf.Epsilon)
        {
            return;
        }

        for (float time = 0f; time <= timelineSystem.Duration + 0.001f; time += interval)
        {
            RectTransform line = Instantiate(linePrefab, trackBar);
            line.gameObject.SetActive(true);
            SetHandlePosition(line, time);
            gridLines.Add(line);
        }
    }

    void SetHandlePosition(RectTransform handle, float timeInSeconds)
    {
        if (handle == null || trackBar == null || timelineSystem == null)
        {
            return;
        }

        handle.SetParent(trackBar, false);
        handle.anchorMin = new Vector2(0f, 0.5f);
        handle.anchorMax = new Vector2(0f, 0.5f);
        handle.pivot = new Vector2(0.5f, 0.5f);
        handle.localScale = Vector3.one;

        float normalized = timelineSystem.TimeToNormalized(timeInSeconds);
        float barWidth = GetTrackBarWidth();
        handle.anchoredPosition = new Vector2(normalized * barWidth, 0f);
    }

    void ClearHandles()
    {
        foreach (KeyValuePair<string, DraggableKeyframe> pair in handles)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value.gameObject);
            }
        }

        handles.Clear();
    }

    void ClearGridLines()
    {
        for (int i = 0; i < gridLines.Count; i++)
        {
            if (gridLines[i] != null)
            {
                Destroy(gridLines[i].gameObject);
            }
        }

        gridLines.Clear();
    }
}
