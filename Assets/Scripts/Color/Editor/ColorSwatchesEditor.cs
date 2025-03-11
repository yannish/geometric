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

    public void OnValidate()
    {
        Debug.LogWarning("ColorSwatchesEditor.OnValidate");
        SceneView.RepaintAll();
    }
}
