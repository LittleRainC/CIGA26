using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TimelineTrack))]
[RequireComponent(typeof(Collider2D))]
public class TimelineTrackSceneDrag : MonoBehaviour
{
    TimelineTrack track;
    TimelinePanel timelinePanel;
    bool isDragging;

    void Awake()
    {
        track = GetComponent<TimelineTrack>();
        timelinePanel = TimelinePanel.Instance;
    }

    void OnMouseDown()
    {
        if (TimelineSystem.Instance != null && TimelineSystem.Instance.IsPlaying)
        {
            return;
        }

        if (IsPointerOverUi())
        {
            return;
        }

        isDragging = true;
        TimelineSystem.Instance?.Pause();
    }

    void OnMouseUp()
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;

        if (timelinePanel == null)
        {
            timelinePanel = TimelinePanel.Instance;
        }

        if (timelinePanel != null && timelinePanel.IsPointerOverDropZone(Input.mousePosition))
        {
            timelinePanel.TryAddDisplayedTrack(track);
        }
    }

    static bool IsPointerOverUi()
    {
        return UnityEngine.EventSystems.EventSystem.current != null
            && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}
