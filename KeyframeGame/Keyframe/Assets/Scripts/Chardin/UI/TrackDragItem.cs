using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrackDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static TimelineTrack DraggingTrack { get; private set; }

    [SerializeField] TimelineTrack track;
    [SerializeField] Text labelText;
    [SerializeField] CanvasGroup canvasGroup;

    public TimelineTrack Track => track;

    public void Bind(TimelineTrack boundTrack)
    {
        track = boundTrack;

        if (labelText != null && track != null)
        {
            labelText.text = track.DisplayName;
        }
    }

    public static void ClearDraggingTrack()
    {
        DraggingTrack = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (track == null)
        {
            return;
        }

        DraggingTrack = track;
        TimelineSystem.Instance?.Pause();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        if (DraggingTrack == track)
        {
            DraggingTrack = null;
        }
    }
}
