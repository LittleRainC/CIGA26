using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackRowUI : MonoBehaviour
{
    [SerializeField] Text labelText;
    [SerializeField] RectTransform trackBar;
    [SerializeField] DraggableKeyframe keyframeHandlePrefab;

    readonly Dictionary<string, DraggableKeyframe> handles = new Dictionary<string, DraggableKeyframe>();

    TimelineTrack track;
    TimelineSystem timelineSystem;

    public TimelineTrack Track => track;

    public void Bind(TimelineTrack boundTrack, TimelineSystem system)
    {
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

        RebuildHandles();
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

    void SetHandlePosition(RectTransform handle, float timeInSeconds)
    {
        if (handle == null || trackBar == null || timelineSystem == null)
        {
            return;
        }

        float normalized = timelineSystem.TimeToNormalized(timeInSeconds);
        Vector2 anchoredPosition = handle.anchoredPosition;
        anchoredPosition.x = normalized * trackBar.rect.width;
        handle.anchoredPosition = anchoredPosition;
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
}
