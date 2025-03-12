using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuickMono))]
public class QuickMonoInspector : Editor
{
    private SerializedProperty quickDataProp;
    private SerializedProperty quickDataArrayProp;
    
    private SerializedProperty nestedFloatProp;
    
    private void OnEnable()
    {
        quickDataProp = serializedObject.FindProperty("quickMonoData");
        quickDataArrayProp = serializedObject.FindProperty("quickMonoDataArray");
        nestedFloatProp = quickDataProp.FindPropertyRelative("quickFloat");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawQuickDataProperty(quickDataProp);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(20f);
        EditorGUILayout.LabelField("And now the normal stuff");
        
        EditorGUILayout.LabelField(
            "Newlines. \n how do they work?", 
            GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f) 
            );

        // DrawDefaultInspector();
        
        // EditorGUILayout.PropertyField(quickDataProp);
        // EditorGUILayout.PropertyField(quickDataArrayProp);
        //
        // EditorGUILayout.Space(20f);
        // EditorGUILayout.LabelField("And now the nested stuff");
        //
        // EditorGUILayout.PropertyField(nestedFloatProp);
    }

    void DrawQuickDataProperty(SerializedProperty prop)
    {
        var boolProp = prop.FindPropertyRelative("quickBool");
        EditorGUILayout.PropertyField(boolProp);
        if (boolProp.boolValue)
        {
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("quickFloat"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("quickInt"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("quickString"));
        }
    }
}
