using UnityEngine;
using UnityEngine.UI;

public class TrackRowUI : MonoBehaviour
{
    [SerializeField] Text labelText;
    [SerializeField] RectTransform trackBar;
    [SerializeField] RectTransform startAnchorHandle;
    [SerializeField] RectTransform endAnchorHandle;
    [SerializeField] DraggableAnchor startAnchorDrag;
    [SerializeField] DraggableAnchor endAnchorDrag;

    TimelineTrack track;

    public TimelineTrack Track => track;

    public void Bind(TimelineTrack boundTrack)
    {
        track = boundTrack;
        startAnchorDrag.Initialize(this, trackBar, true);
        endAnchorDrag.Initialize(this, trackBar, false);
        Refresh();
    }

    public void Refresh()
    {
        if (track == null || trackBar == null)
        {
            return;
        }

        if (labelText != null)
        {
            labelText.text = track.DisplayName;
        }

        SetAnchorVisual(startAnchorHandle, track.StartAnchorTime);
        SetAnchorVisual(endAnchorHandle, track.EndAnchorTime);
    }

    public void SetAnchorTime(bool isStartAnchor, float normalizedTime)
    {
        if (track == null)
        {
            return;
        }

        if (isStartAnchor)
        {
            track.SetStartAnchorTime(normalizedTime);
        }
        else
        {
            track.SetEndAnchorTime(normalizedTime);
        }

        Refresh();
    }

    void SetAnchorVisual(RectTransform anchor, float normalizedTime)
    {
        if (anchor == null || trackBar == null)
        {
            return;
        }

        // Track bar should use left-middle pivot (0, 0.5) for this mapping.
        float x = normalizedTime * trackBar.rect.width;
        Vector2 anchoredPosition = anchor.anchoredPosition;
        anchoredPosition.x = x;
        anchor.anchoredPosition = anchoredPosition;
    }
}
