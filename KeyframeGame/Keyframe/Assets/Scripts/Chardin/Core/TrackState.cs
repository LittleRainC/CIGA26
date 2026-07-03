using System;
using UnityEngine;

[Serializable]
public struct TrackState
{
    public Vector3 position;
    public float rotationZ;

    public static TrackState FromTransform(Transform transform)
    {
        return new TrackState
        {
            position = transform.position,
            rotationZ = transform.eulerAngles.z
        };
    }

    public static TrackState Lerp(TrackState from, TrackState to, float t)
    {
        t = Mathf.Clamp01(t);
        return new TrackState
        {
            position = Vector3.Lerp(from.position, to.position, t),
            rotationZ = Mathf.LerpAngle(from.rotationZ, to.rotationZ, t)
        };
    }

    public void ApplyTo(Transform transform)
    {
        transform.position = position;
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
    }
}
