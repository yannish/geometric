using System;
using NUnit.Framework.Constraints;
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
    private SerializedProperty prefabProp;    
    
    private BoxBrushDecorator decorator;
    private BoxColliderExtendedEditor boxColliderExtendedEditor;
    private bool objectDirtied = false;

    
    private void OnEnable()
    {
        decorator = target as BoxBrushDecorator;
        
        typeProp = serializedObject.FindProperty("type");

        faceStatesProp = serializedObject.FindProperty("faceStates");
        dimsProp = serializedObject.FindProperty("dimensions");
        debugProp = serializedObject.FindProperty("debug");
        prefabProp = serializedObject.FindProperty("prefab");
        
        ActiveEditorTracker editorTracker = ActiveEditorTracker.sharedTracker;
        Editor[] editors = editorTracker.activeEditors;

        foreach (var editor in editors)
        {
            if (editor.target is BoxCollider)
            {
                boxColliderExtendedEditor = editor as BoxColliderExtendedEditor;
                boxColliderExtendedEditor.OnBoxColliderChanged -= HandleBoxColliderEdit;
                boxColliderExtendedEditor.OnBoxColliderChanged += HandleBoxColliderEdit;
            }
        }
    }

    private void OnDisable()
    {
        if(boxColliderExtendedEditor != null)
            boxColliderExtendedEditor.OnBoxColliderChanged -= HandleBoxColliderEdit;
    }

    private void HandleBoxColliderEdit()
    {
        
    }

    public override void OnInspectorGUI()
    {
        objectDirtied = false;
        
        serializedObject.DrawScriptField();
        serializedObject.Update();
        
        EditorGUI.BeginChangeCheck();
        
        EditorGUILayout.PropertyField(debugProp);
        EditorGUILayout.PropertyField(typeProp);
        EditorGUILayout.PropertyField(dimsProp);
        EditorGUILayout.PropertyField(prefabProp);
        
        DrawFaceContent();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            foreach (var face in decorator.faceStates)
            {
                if (face.isMuted)
                    continue;
            
                if (BoxBrushDecoratorActions.RecalculateDecoratorFace(decorator, face))
                    BoxBrushDecoratorActions.RegeneratePrefabInstances(decorator, face);
                
                BoxBrushDecoratorActions.RealignDecoratorFaceInstances(decorator, face);
            }
        }
    }

    public void OnSceneGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Keypad1 )// && e.modifiers == EventModifiers.Shift)
        {
            Debug.LogWarning("Pressed 1 key!");
        }
        
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Keypad2)
        {
            Debug.LogWarning("Pressed 2 key!");
        }
        
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Keypad3)
        {
            Debug.LogWarning("Pressed 3 key!");
        }
    }

    void DrawFaceContent()
    {
        EditorGUILayout.PropertyField(faceStatesProp);
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


}
