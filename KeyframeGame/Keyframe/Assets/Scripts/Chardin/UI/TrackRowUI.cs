using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    RectTransform timelineContentReference;

    public TimelineTrack Track => track;

    public void Bind(
        TimelineTrack boundTrack,
        TimelineSystem system,
        TimelineLayoutSettings layoutSettings,
        RectTransform contentReference)
    {
        layout = layoutSettings;
        timelineContentReference = contentReference;

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

        AlignRowToTimelineContent();
        SetupTrackBarLayout();
        RebuildGridLines();
        RebuildHandles();
    }

    public void RefreshLayout()
    {
        AlignRowToTimelineContent();
        SetupTrackBarLayout();
        RebuildGridLines();
        RebuildHandles();
    }

    void AlignRowToTimelineContent()
    {
        if (timelineContentReference == null)
        {
            SetupLabelLayout();
            return;
        }

        RectTransform rowRect = transform as RectTransform;
        if (rowRect == null || trackBar == null)
        {
            return;
        }

        Vector3[] refCorners = new Vector3[4];
        Vector3[] rowCorners = new Vector3[4];
        timelineContentReference.GetWorldCorners(refCorners);
        rowRect.GetWorldCorners(rowCorners);

        float refLeft = refCorners[0].x;
        float refRight = refCorners[2].x;
        float rowLeft = rowCorners[0].x;

        float labelWidth = (refLeft - rowLeft) / rowRect.lossyScale.x;
        float contentWidth = (refRight - refLeft) / timelineContentReference.lossyScale.x;

        layout = new TimelineLayoutSettings(Mathf.Max(0f, labelWidth), Mathf.Max(0f, contentWidth));
        SetupLabelLayout();
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
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = new Vector2(layout.labelColumnWidth, labelRect.sizeDelta.y);
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

        if (track == null || trackBar == null || keyframeHandlePrefab == null)
        {
            return;
        }

        IReadOnlyList<TimelineKeyframe> keyframes = track.Keyframes;
        for (int i = 0; i < keyframes.Count; i++)
        {
            TimelineKeyframe keyframe = keyframes[i];
            DraggableKeyframe handle = Instantiate(keyframeHandlePrefab, trackBar);
            handle.Initialize(this, keyframe.Id, trackBar);
            handles[keyframe.Id] = handle;
            SetHandlePosition(handle.GetComponent<RectTransform>(), keyframe.Time);
        }
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

        if (timelineSystem == null || trackBar == null || gridLinePrefab == null)
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
            RectTransform line = Instantiate(gridLinePrefab, trackBar);
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
        handle.pivot = new Vector2(0f, 0.5f);

        float normalized = timelineSystem.TimeToNormalized(timeInSeconds);
        handle.anchoredPosition = new Vector2(normalized * trackBar.rect.width, 0f);
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
