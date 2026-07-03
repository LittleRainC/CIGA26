using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrackDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] TimelinePanel timelinePanel;

    void Awake()
    {
        if (timelinePanel == null)
        {
            timelinePanel = TimelinePanel.Instance;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (timelinePanel == null)
        {
            timelinePanel = TimelinePanel.Instance;
        }

        TimelineTrack track = TrackDragItem.DraggingTrack;
        if (track == null)
        {
            return;
        }

        timelinePanel?.HandleTrackDrop(track);
        TrackDragItem.ClearDraggingTrack();
    }
}
