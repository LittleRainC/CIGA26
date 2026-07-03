using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TimelineTrack))]
public class TimelineTrackEditor : Editor
{
    SerializedProperty displayNameProperty;
    SerializedProperty keyframesProperty;

    float newKeyframeTime = 1f;

    void OnEnable()
    {
        displayNameProperty = serializedObject.FindProperty("displayName");
        keyframesProperty = serializedObject.FindProperty("keyframes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(displayNameProperty);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Keyframes", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(keyframesProperty, true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

        newKeyframeTime = EditorGUILayout.FloatField("New Keyframe Time (s)", newKeyframeTime);

        TimelineTrack track = (TimelineTrack)target;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Keyframe From Current Transform"))
        {
            Undo.RecordObject(track, "Add Timeline Keyframe");
            track.CaptureKeyframeFromTransform(Mathf.Max(0f, newKeyframeTime));
            EditorUtility.SetDirty(track);
            serializedObject.Update();
        }

        if (GUILayout.Button("Remove Last Keyframe") && track.Keyframes.Count > 0)
        {
            Undo.RecordObject(track, "Remove Timeline Keyframe");
            track.RemoveKeyframe(track.Keyframes[track.Keyframes.Count - 1].Id);
            EditorUtility.SetDirty(track);
            serializedObject.Update();
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}
