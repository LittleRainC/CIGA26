using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableKeyframe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    RectTransform trackBar;
    TrackRowUI rowUI;
    Canvas rootCanvas;

    string keyframeId;
    float pendingTime;
    bool isDragging;

    public string KeyframeId => keyframeId;
    public bool IsDragging => isDragging;

    public void Initialize(TrackRowUI row, string id, RectTransform bar)
    {
        rowUI = row;
        keyframeId = id;
        trackBar = bar;
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        TimelineSystem.Instance?.Pause();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rowUI == null || trackBar == null || TimelineSystem.Instance == null)
        {
            return;
        }

        if (!TryGetTimeFromPointer(eventData, out pendingTime))
        {
            return;
        }

        rowUI.UpdateHandlePreview(keyframeId, pendingTime);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        if (rowUI == null || trackBar == null || TimelineSystem.Instance == null)
        {
            return;
        }

        if (!TryGetTimeFromPointer(eventData, out pendingTime))
        {
            return;
        }

        rowUI.CommitKeyframeTime(keyframeId, pendingTime);
    }

    bool TryGetTimeFromPointer(PointerEventData eventData, out float timeInSeconds)
    {
        timeInSeconds = 0f;

        Camera eventCamera = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? rootCanvas.worldCamera
            : null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                trackBar,
                eventData.position,
                eventCamera,
                out Vector2 localPoint))
        {
            return false;
        }

        float normalized = Mathf.Clamp01(localPoint.x / trackBar.rect.width);
        timeInSeconds = TimelineSystem.Instance.NormalizedToTime(normalized);
        return true;
    }
}
