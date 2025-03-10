using System;
// using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ColorSwatches))]
public class ColorSwatchesEditor : Editor
{
    private SerializedProperty testFloatProp;

    private void OnEnable()
    {
        testFloatProp = serializedObject.FindProperty("testFloat");

        SceneView.duringSceneGui += DuringSceneGUI;
    }


    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }
    
    private void DuringSceneGUI(SceneView obj)
    {
        
    }

    private void OnSceneGUI()
    {
        
    }

    // public override void OnInspectorGUI()
    // {
    //     // serializedObject.Update();
    //     // EditorGUILayout.PropertyField(testFloatProp);
    //     // if (serializedObject.ApplyModifiedProperties())
    //     // {
    //     //     Debug.LogWarning("ApplyModifiedProperties!");
    //     // }
    //     
    //     EditorGUI.BeginChangeCheck();
    //     
    //     base.OnInspectorGUI();
    //     
    //     GUILayout.Label("HAI");
    //     
    //     if (EditorGUI.EndChangeCheck())
    //     {
    //         Debug.LogWarning("ApplyModifiedProperties!");
    //         SceneView.RepaintAll();
    //     }
    // }

    public void OnValidate()
    {
        Debug.LogWarning("ColorSwatchesEditor.OnValidate");
        SceneView.RepaintAll();
    }
}
