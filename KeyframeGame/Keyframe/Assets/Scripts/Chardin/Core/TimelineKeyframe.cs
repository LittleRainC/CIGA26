using System;
using UnityEngine;

[Serializable]
public class TimelineKeyframe
{
    [SerializeField] string id;
    [SerializeField] float time;
    [SerializeField] TrackState state;

    public string Id => id;
    public float Time => time;
    public TrackState State => state;

    public TimelineKeyframe(float time, TrackState state)
    {
        id = Guid.NewGuid().ToString("N");
        this.time = time;
        this.state = state;
    }

    public void SetTime(float newTime)
    {
        time = newTime;
    }

    public void SetState(TrackState newState)
    {
        state = newState;
    }
}
