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
    private SerializedProperty calculatedPrefabSizeProp;
    
    private BoxBrushDecorator decorator;
    private BoxColliderExtendedEditor boxColliderExtendedEditor;
    // private bool objectDirtied = false;

    
    private void OnEnable()
    {
        decorator = target as BoxBrushDecorator;
        
        typeProp = serializedObject.FindProperty("type");

        faceStatesProp = serializedObject.FindProperty("faceStates");
        dimsProp = serializedObject.FindProperty("dimensions");
        debugProp = serializedObject.FindProperty("debug");
        prefabProp = serializedObject.FindProperty("prefab");
        calculatedPrefabSizeProp = serializedObject.FindProperty("calculatedPrefabSize");
        
        ActiveEditorTracker editorTracker = ActiveEditorTracker.sharedTracker;
        Editor[] editors = editorTracker.activeEditors;

        foreach (var editor in editors)
        {
            if (editor.target is BoxCollider)
            {
                // TODO: .. destroy / shut this down as well...?
                boxColliderExtendedEditor = editor as BoxColliderExtendedEditor;
                boxColliderExtendedEditor.OnBoxColliderChanged -= HandleBoxColliderEdit;
                boxColliderExtendedEditor.OnBoxColliderChanged += HandleBoxColliderEdit;
                break;
            }
        }
        
        //... TODO: recalculate all on select?
        // BoxBrushDecoratorActions.RecalculateDecoratorFace()
    }

    private void OnDisable()
    {
        if(boxColliderExtendedEditor != null)
            boxColliderExtendedEditor.OnBoxColliderChanged -= HandleBoxColliderEdit;
    }

    private void HandleBoxColliderEdit()
    {
        
    }

    private bool changedPrefabAsset;
    
    public override void OnInspectorGUI()
    {
        // objectDirtied = false;

        changedPrefabAsset = false;
        
        serializedObject.DrawScriptField();
        serializedObject.Update();

        if (GUILayout.Button("CLEAR ALL"))
        {
            foreach (var face in decorator.faceStates)
            {
                foreach (var instance in face.instances)
                {
                    DestroyImmediate(instance);
                }
                
                face.instances.Clear();
            }
        }
        
        EditorGUI.BeginChangeCheck();
        
        EditorGUILayout.PropertyField(debugProp);
        EditorGUILayout.PropertyField(dimsProp);

        var prevDecoratorType = decorator.type;
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(typeProp);
        if (EditorGUI.EndChangeCheck())
        {
            
        }
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(prefabProp);
        if (EditorGUI.EndChangeCheck())
        {
            // serializedObject.ApplyModifiedProperties();
            Debug.LogWarning("Changed prefab asset.");
            changedPrefabAsset = true;
        }
        
        EditorGUILayout.PropertyField(calculatedPrefabSizeProp);
        
        DrawFaceContent();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();

            //... switched off Faces
            if (
                changedPrefabAsset
                || (prevDecoratorType == BoxBrushDecorationType.FACE && decorator.type != BoxBrushDecorationType.FACE)
                )
            {
                Debug.LogWarning("CLEARING FACES");
                
                foreach (var face in decorator.faceStates)
                {
                    BoxBrushDecoratorActions.RecalculateDecoratorFace(decorator, face);
                    BoxBrushDecoratorActions.ClearDecoratorFaceInstance(decorator, face);
                }
            }

            //... switched to Faces
            if (prevDecoratorType != BoxBrushDecorationType.FACE && decorator.type == BoxBrushDecorationType.FACE)
            {
                foreach (var face in decorator.faceStates)
                {
                    if (face.isMuted)
                    {
                        BoxBrushDecoratorActions.RecalculateDecoratorFace(decorator, face);
                        BoxBrushDecoratorActions.ClearDecoratorFaceInstance(decorator, face);
                        continue;
                    }

                    bool instanceCountChange = BoxBrushDecoratorActions.RecalculateDecoratorFace(decorator, face);
                
                    if (
                        instanceCountChange
                        || face.instances == null
                        || face.instances.Count != face.positions.Count
                    )
                    {
                        BoxBrushDecoratorActions.ClearDecoratorFaceInstance(decorator, face);
                        BoxBrushDecoratorActions.RegeneratePrefabInstances(decorator, face);
                    }
                
                    BoxBrushDecoratorActions.RealignDecoratorFaceInstances(decorator, face);
                }
            }
            
            //... just update Faces
            if (decorator.type == BoxBrushDecorationType.FACE)
            {
                foreach (var face in decorator.faceStates)
                {
                    if (face.isMuted)
                    {
                        BoxBrushDecoratorActions.RecalculateDecoratorFace(decorator, face);
                        BoxBrushDecoratorActions.ClearDecoratorFaceInstance(decorator, face);
                        continue;
                    }

                    bool instanceCountChange = BoxBrushDecoratorActions.RecalculateDecoratorFace(decorator, face);
                    
                    if (
                        instanceCountChange
                        || face.instances == null
                        || face.instances.Count != face.positions.Count
                    )
                    {
                        BoxBrushDecoratorActions.ClearDecoratorFaceInstance(decorator, face);
                        BoxBrushDecoratorActions.RegeneratePrefabInstances(decorator, face);
                    }
                    
                    BoxBrushDecoratorActions.RealignDecoratorFaceInstances(decorator, face);
                }
            }
        }
    }

    void ClearFaces()
    {
        foreach(var face in decorator.faceStates)
            BoxBrushDecoratorActions.ClearDecoratorFaceInstance(decorator, face);
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
