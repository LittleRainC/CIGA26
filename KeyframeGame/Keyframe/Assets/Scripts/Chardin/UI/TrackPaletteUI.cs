using System.Collections.Generic;
using UnityEngine;

public class TrackPaletteUI : MonoBehaviour
{
    [SerializeField] TimelineSystem timelineSystem;
    [SerializeField] TimelinePanel timelinePanel;
    [SerializeField] RectTransform itemContainer;
    [SerializeField] TrackDragItem trackDragItemPrefab;

    readonly List<TrackDragItem> items = new List<TrackDragItem>();

    void Awake()
    {
        if (timelineSystem == null)
        {
            timelineSystem = TimelineSystem.Instance;
        }

        if (timelinePanel == null)
        {
            timelinePanel = TimelinePanel.Instance;
        }
    }

    void OnEnable()
    {
        if (timelineSystem != null)
        {
            timelineSystem.TracksChanged += RebuildPalette;
        }

        if (timelinePanel != null)
        {
            timelinePanel.DisplayedTracksChanged += RebuildPalette;
        }

        RebuildPalette();
    }

    void OnDisable()
    {
        if (timelineSystem != null)
        {
            timelineSystem.TracksChanged -= RebuildPalette;
        }

        if (timelinePanel != null)
        {
            timelinePanel.DisplayedTracksChanged -= RebuildPalette;
        }
    }

    void RebuildPalette()
    {
        ClearItems();

        if (timelineSystem == null || trackDragItemPrefab == null || itemContainer == null)
        {
            return;
        }

        IReadOnlyList<TimelineTrack> tracks = timelineSystem.Tracks;
        for (int i = 0; i < tracks.Count; i++)
        {
            TimelineTrack track = tracks[i];
            if (timelinePanel != null && timelinePanel.IsTrackDisplayed(track))
            {
                continue;
            }

            TrackDragItem item = Instantiate(trackDragItemPrefab, itemContainer);
            item.Bind(track);
            items.Add(item);
        }
    }

    void ClearItems()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null)
            {
                Destroy(items[i].gameObject);
            }
        }

        items.Clear();
    }
}
