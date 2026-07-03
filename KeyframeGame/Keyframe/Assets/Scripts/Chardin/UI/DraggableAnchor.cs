using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableAnchor : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [SerializeField] RectTransform trackBar;
    [SerializeField] bool isStartAnchor = true;

    TrackRowUI rowUI;
    Canvas rootCanvas;
    bool isDragging;

    public void Initialize(TrackRowUI row, RectTransform bar, bool startAnchor)
    {
        rowUI = row;
        trackBar = bar;
        isStartAnchor = startAnchor;
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        TimelineSystem.Instance?.Pause();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rowUI == null || trackBar == null)
        {
            return;
        }

        Camera eventCamera = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? rootCanvas.worldCamera
            : null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                trackBar,
                eventData.position,
                eventCamera,
                out Vector2 localPoint))
        {
            return;
        }

        float normalizedTime = Mathf.Clamp01(localPoint.x / trackBar.rect.width);
        rowUI.SetAnchorTime(isStartAnchor, normalizedTime);
    }

    void OnDisable()
    {
        isDragging = false;
    }

    public bool IsDragging => isDragging;
}
