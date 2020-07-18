using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioData), true)]
public class AudioEditor : Editor
{
    private AudioSource preview;
    public void OnEnable() {
        preview = EditorUtility.CreateGameObjectWithHideFlags("Audio Preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();

    }

    public void OnDisable() {
        DestroyImmediate(preview.gameObject);
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
        if (GUILayout.Button("test sound")) {
            ((AudioData)target).Play(preview);
        }
        EditorGUI.EndDisabledGroup();

        
    }


    
}
