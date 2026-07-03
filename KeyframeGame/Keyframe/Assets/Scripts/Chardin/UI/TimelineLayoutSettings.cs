using UnityEngine;

public struct TimelineLayoutSettings
{
    public float labelColumnWidth;
    public float timelineContentWidth;

    public TimelineLayoutSettings(float labelColumnWidth, float timelineContentWidth)
    {
        this.labelColumnWidth = labelColumnWidth;
        this.timelineContentWidth = timelineContentWidth;
    }
}
