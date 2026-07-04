using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(0)]
public class TimelineTrack : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] string displayName;

    [Header("Keyframes")]
    [SerializeField] List<TimelineKeyframe> keyframes = new List<TimelineKeyframe>();

    readonly List<TimelineKeyframe> sortedBuffer = new List<TimelineKeyframe>();

    const float MinKeyframeSeparation = 0.1f;

    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public IReadOnlyList<TimelineKeyframe> Keyframes => keyframes;

    public event Action KeyframesChanged;

    void OnEnable()
    {
        if (TimelineSystem.Instance != null)
        {
            TimelineSystem.Instance.RegisterTrack(this);
            ApplyTime(TimelineSystem.Instance.CurrentTime);
        }
    }

    void Start()
    {
        TimelineSystem.Instance?.RegisterTrack(this);
        ApplyTime(TimelineSystem.Instance != null ? TimelineSystem.Instance.CurrentTime : 0f);
    }

    void OnDisable()
    {
        TimelineSystem.Instance?.UnregisterTrack(this);
    }

    void OnDestroy()
    {
        TimelineSystem.Instance?.UnregisterTrack(this);
    }

    public void Evaluate(float currentTime)
    {
        ApplyTime(currentTime);
    }

    void ApplyTime(float currentTime)
    {
        if (keyframes.Count == 0)
        {
            return;
        }

        BuildSortedKeyframes();

        if (sortedBuffer.Count == 1)
        {
            sortedBuffer[0].State.ApplyTo(transform);
            return;
        }

        TimelineKeyframe first = sortedBuffer[0];
        TimelineKeyframe last = sortedBuffer[sortedBuffer.Count - 1];

        if (currentTime <= first.Time)
        {
            first.State.ApplyTo(transform);
            return;
        }

        if (currentTime >= last.Time)
        {
            last.State.ApplyTo(transform);
            return;
        }

        for (int i = 0; i < sortedBuffer.Count - 1; i++)
        {
            TimelineKeyframe left = sortedBuffer[i];
            TimelineKeyframe right = sortedBuffer[i + 1];

            if (currentTime < left.Time || currentTime > right.Time)
            {
                continue;
            }

            float span = right.Time - left.Time;
            float t = span <= Mathf.Epsilon ? 1f : (currentTime - left.Time) / span;
            TrackState.Lerp(left.State, right.State, t).ApplyTo(transform);
            return;
        }
    }

    public void SetKeyframeTime(string keyframeId, float newTime)
    {
        TimelineKeyframe keyframe = FindKeyframe(keyframeId);
        if (keyframe == null)
        {
            return;
        }

        TimelineSystem system = TimelineSystem.Instance;
        float duration = system != null ? system.Duration : Mathf.Max(newTime, 1f);
        float snappedTime = system != null ? system.SnapTime(newTime) : newTime;
        snappedTime = ResolveGridTime(keyframeId, snappedTime, newTime, duration);

        keyframe.SetTime(Mathf.Clamp(snappedTime, 0f, duration));
        KeyframesChanged?.Invoke();
    }

    float ResolveGridTime(string movedKeyframeId, float snappedTime, float dragReferenceTime, float duration)
    {
        if (!HasTimeConflict(movedKeyframeId, snappedTime))
        {
            return snappedTime;
        }

        TimelineSystem system = TimelineSystem.Instance;
        float gridStep = system != null ? system.GridInterval : 5f;
        if (gridStep <= Mathf.Epsilon)
        {
            return snappedTime;
        }

        float candidateUp = Mathf.Clamp(snappedTime + gridStep, 0f, duration);
        float candidateDown = Mathf.Clamp(snappedTime - gridStep, 0f, duration);

        TimelineKeyframe neighbor = GetClosestOtherKeyframe(movedKeyframeId, snappedTime);
        bool preferUp = neighbor == null || dragReferenceTime >= neighbor.Time;

        if (preferUp)
        {
            if (candidateUp != snappedTime && !HasTimeConflict(movedKeyframeId, candidateUp))
            {
                return candidateUp;
            }

            if (candidateDown != snappedTime && !HasTimeConflict(movedKeyframeId, candidateDown))
            {
                return candidateDown;
            }
        }
        else
        {
            if (candidateDown != snappedTime && !HasTimeConflict(movedKeyframeId, candidateDown))
            {
                return candidateDown;
            }

            if (candidateUp != snappedTime && !HasTimeConflict(movedKeyframeId, candidateUp))
            {
                return candidateUp;
            }
        }

        return snappedTime;
    }

    bool HasTimeConflict(string movedKeyframeId, float time)
    {
        for (int i = 0; i < keyframes.Count; i++)
        {
            TimelineKeyframe other = keyframes[i];
            if (other.Id == movedKeyframeId)
            {
                continue;
            }

            if (Mathf.Abs(other.Time - time) < MinKeyframeSeparation)
            {
                return true;
            }
        }

        return false;
    }

    TimelineKeyframe GetClosestOtherKeyframe(string movedKeyframeId, float time)
    {
        TimelineKeyframe closest = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < keyframes.Count; i++)
        {
            TimelineKeyframe other = keyframes[i];
            if (other.Id == movedKeyframeId)
            {
                continue;
            }

            float distance = Mathf.Abs(other.Time - time);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = other;
            }
        }

        return closest;
    }

    public void AddKeyframe(float time, TrackState state)
    {
        float duration = TimelineSystem.Instance != null ? TimelineSystem.Instance.Duration : Mathf.Max(time, 1f);
        float snappedTime = TimelineSystem.Instance != null
            ? TimelineSystem.Instance.SnapTime(time)
            : time;
        keyframes.Add(new TimelineKeyframe(Mathf.Clamp(snappedTime, 0f, duration), state));
        KeyframesChanged?.Invoke();
    }

    public void RemoveKeyframe(string keyframeId)
    {
        for (int i = keyframes.Count - 1; i >= 0; i--)
        {
            if (keyframes[i].Id == keyframeId)
            {
                keyframes.RemoveAt(i);
                KeyframesChanged?.Invoke();
                return;
            }
        }
    }

    public void CaptureKeyframeFromTransform(float time)
    {
        AddKeyframe(time, TrackState.FromTransform(transform));
    }

    TimelineKeyframe FindKeyframe(string keyframeId)
    {
        for (int i = 0; i < keyframes.Count; i++)
        {
            if (keyframes[i].Id == keyframeId)
            {
                return keyframes[i];
            }
        }

        return null;
    }

    void BuildSortedKeyframes()
    {
        sortedBuffer.Clear();
        sortedBuffer.AddRange(keyframes);
        sortedBuffer.Sort((a, b) => a.Time.CompareTo(b.Time));
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (keyframes == null || keyframes.Count == 0)
        {
            return;
        }

        BuildSortedKeyframes();
        for (int i = 0; i < sortedBuffer.Count; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(sortedBuffer[i].State.position, 0.12f);
            if (i < sortedBuffer.Count - 1)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(sortedBuffer[i].State.position, sortedBuffer[i + 1].State.position);
            }
        }
    }
#endif
}
