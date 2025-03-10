using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoxBrushDecorator))]
public class BoxBrushDecoratorInspector : Editor
{
    private SerializedProperty typeProp;
    // private SerializedProperty faceFlagsProp;
    private SerializedProperty faceStatesProp;
    private SerializedProperty dimsProp;
    private SerializedProperty debugProp;
    private BoxBrushDecorator decorator;
    
    private void OnEnable()
    {
        decorator = target as BoxBrushDecorator;
        
        typeProp = serializedObject.FindProperty("type");

        faceStatesProp = serializedObject.FindProperty("faceStates");
        dimsProp = serializedObject.FindProperty("dimensions");
        debugProp = serializedObject.FindProperty("debug");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(debugProp);
        EditorGUILayout.PropertyField(typeProp);
        EditorGUILayout.PropertyField(dimsProp);
        
        DrawFaceContent();
        
        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
            foreach (var face in decorator.faceStates)
            {
                
            }
        }
    }

    void DrawGroups()
    {
        if (decorator.type.HasFlag(BoxBrushDecorationType.EDGE))
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("EDGE:");
                EditorGUILayout.Space();
            }
        }

        if (decorator.type.HasFlag(BoxBrushDecorationType.FACE))
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("FACE:");
                EditorGUILayout.Space();
            }
        }

        if (decorator.type.HasFlag(BoxBrushDecorationType.CORNER))
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("CORNER:");
                EditorGUILayout.Space();
            }
        }
    }
    
    void DrawCornerContent()
    {
        
    }

    void DrawFaceContent()
    {
        // EditorGUILayout.PropertyField(faceFlagsProp);

        // for (int i = 0; i < faceStatesProp.arraySize; i++)
        // {
        //     var prop = faceStatesProp.GetArrayElementAtIndex(i);
        //     var relProp = prop.FindPropertyRelative("isMuted");
        //     EditorGUILayout.PropertyField(relProp);
        //     EditorGUILayout.PropertyField(prop);
        // }
        
        // EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(faceStatesProp);


        // if (EditorGUI.EndChangeCheck())
        // {
        //     Debug.LogWarning("Tweaked something in facestates!");
        //     // bool reinstantiatPrefabs = false;
        //     foreach (var face in decorator.faceStates)
        //     {
        //         if (face.isMuted)
        //             continue;
        //
        //         if (BoxBrushDecoratorActions.RecalculateDecoratorFace(decorator, face))
        //         {
        //             BoxBrushDecoratorActions.RegeneratePrefabInstances(decorator, face);
        //         }
        //         
        //         BoxBrushDecoratorActions.RealignDecoratorFaceInstances(decorator, face);
        //     }
        // }
    }
}
