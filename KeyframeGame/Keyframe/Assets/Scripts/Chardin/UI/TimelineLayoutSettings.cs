using UnityEngine;

public struct TimelineLayoutSettings
{
    public float labelColumnWidth;
    public float timelineContentWidth;
    public float labelOffsetX;

    public TimelineLayoutSettings(float labelColumnWidth, float timelineContentWidth, float labelOffsetX = 0f)
    {
        this.labelColumnWidth = labelColumnWidth;
        this.timelineContentWidth = timelineContentWidth;
        this.labelOffsetX = labelOffsetX;
    }
}
