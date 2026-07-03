using System;
using UnityEngine;

[DisallowMultipleComponent]
public class TimelineTrack : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] string displayName;

    [Header("Anchors (0-1)")]
    [SerializeField, Range(0f, 1f)] float startAnchorTime = 0.2f;
    [SerializeField, Range(0f, 1f)] float endAnchorTime = 0.6f;

    [Header("States")]
    [SerializeField] TrackState startState;
    [SerializeField] TrackState endState;
    [SerializeField] bool captureStartStateOnAwake = true;

    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public float StartAnchorTime => startAnchorTime;
    public float EndAnchorTime => endAnchorTime;
    public TrackState StartState => startState;
    public TrackState EndState => endState;

    public event Action AnchorsChanged;

    void Awake()
    {
        if (captureStartStateOnAwake)
        {
            startState = TrackState.FromTransform(transform);
        }
    }

    void Start()
    {
        TimelineSystem.Instance?.RegisterTrack(this);
    }

    void OnDestroy()
    {
        TimelineSystem.Instance?.UnregisterTrack(this);
    }

    public void Evaluate(float normalizedTime)
    {
        TrackState state;

        if (normalizedTime <= startAnchorTime)
        {
            state = startState;
        }
        else if (normalizedTime >= endAnchorTime)
        {
            state = endState;
        }
        else
        {
            float duration = endAnchorTime - startAnchorTime;
            float t = duration <= Mathf.Epsilon ? 1f : (normalizedTime - startAnchorTime) / duration;
            state = TrackState.Lerp(startState, endState, t);
        }

        state.ApplyTo(transform);
    }

    public void SetStartAnchorTime(float value)
    {
        startAnchorTime = Mathf.Clamp(value, 0f, endAnchorTime);
        AnchorsChanged?.Invoke();
        TimelineSystem.Instance?.NotifyTrackAnchorsChanged();
    }

    public void SetEndAnchorTime(float value)
    {
        endAnchorTime = Mathf.Clamp(value, startAnchorTime, 1f);
        AnchorsChanged?.Invoke();
        TimelineSystem.Instance?.NotifyTrackAnchorsChanged();
    }

    public void SetEndStateFromCurrentTransform()
    {
        endState = TrackState.FromTransform(transform);
    }

    [ContextMenu("Capture End State From Current Transform")]
    void CaptureEndStateContextMenu()
    {
        SetEndStateFromCurrentTransform();
    }

    [ContextMenu("Capture Start State From Current Transform")]
    void CaptureStartStateContextMenu()
    {
        startState = TrackState.FromTransform(transform);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(startState.position, 0.15f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(endState.position, 0.15f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(startState.position, endState.position);
    }
#endif
}
