using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoxBrushDecorator))]
public class BoxBrushDecoratorInspector : Editor
{
    private SerializedProperty typeProp;
    private SerializedProperty faceStatesProp;
    private SerializedProperty cornerStatesProp;
    private SerializedProperty dimsProp;
    private SerializedProperty debugProp;
    private SerializedProperty prefabProp;
    private SerializedProperty calculatedPrefabSizeProp;
    
    private BoxBrushDecorator decorator;
    private BoxColliderExtendedEditor boxColliderExtendedEditor;

    private int selectedCorner = -1;
    private bool decoratorDirtied = false;
    private bool changedPrefabAsset;

    
    private void OnEnable()
    {
        decorator = target as BoxBrushDecorator;
        
        typeProp = serializedObject.FindProperty("type");

        faceStatesProp = serializedObject.FindProperty("faceStates");
        cornerStatesProp = serializedObject.FindProperty("cornerStates");
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
        
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.Focus();
            // sceneViewFocused = true; // Prevent excessive focusing
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

    
    public override void OnInspectorGUI()
    {
        decoratorDirtied = false;
        changedPrefabAsset = false;
        
        serializedObject.DrawScriptField();
        serializedObject.Update();

        
        // if (GUILayout.Button("CLEAR ALL"))
        // {
        //     foreach (var face in decorator.faceStates)
        //     {
        //         foreach (var instance in face.instances)
        //         {
        //             DestroyImmediate(instance);
        //         }
        //         
        //         face.instances.Clear();
        //     }
        // }
        
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
        EditorGUILayout.PropertyField(faceStatesProp);
        EditorGUILayout.PropertyField(cornerStatesProp);
        
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();

            //... CORNERS:
            // if (
            //     changedPrefabAsset
            //     || (prevDecoratorType == BoxBrushDecorationType.CORNER && decorator.type != BoxBrushDecorationType.CORNER)
            // )
            // {
            //     Debug.LogWarning("CLEARING CORNERS!");
            //     decorator.RecalculateCorners();
            //     decorator.ClearCorners();
            // }
            //
            // if (prevDecoratorType != BoxBrushDecorationType.CORNER && decorator.type == BoxBrushDecorationType.CORNER)
            // {
            //     decorator.RecalculateCorners();
            //     decorator.UpdateCorners();
            // }
            //
            // if (decorator.type == BoxBrushDecorationType.CORNER)
            // {
            //     decorator.RecalculateCorners();
            //     decorator.UpdateCorners();
            // }
            
            
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

        if (decoratorDirtied)
        {
            // serializedObject.ApplyModifiedProperties();
            decoratorDirtied = false;
        }
    }



    public void OnSceneGUI()
    {
        TickOnInspectorInput();
        TickSceneViewInput();   

        DrawCornerControls();
        DrawFaceControls();
        DrawEdgeControls();
    }

    private void DrawEdgeControls()
    {
        
    }

    private void DrawFaceControls()
    {
        
    }

    
    void DrawCornerControls()
    {
        if (decorator.type != BoxBrushDecorationType.CORNER)
            return;
        
        var prevHandlesMatrix = Handles.matrix;
        Handles.matrix = decorator.transform.localToWorldMatrix;
        
        foreach (var kvp in BoxBrushDirections.cornerNormalLookup)
        {
            var dir = kvp.Value;
            var handlePos = Vector3.Scale(dir, decorator.dims * 0.5f);
            var handleRot = Quaternion.LookRotation(dir, Vector3.up);
            
            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            float effectiveHandleSize = decorator.debug.elementSelectionSize;
            effectiveHandleSize *= (int)kvp.Key == selectedCorner ? decorator.debug.elementSelectedInflation : 1f;
            
            if (Handles.Button(
                    handlePos,
                    handleRot,
                    effectiveHandleSize,
                    decorator.debug.elementPickSize,
                    Handles.RectangleHandleCap
                ))
            {
                selectedCorner = (int)kvp.Key;
                SceneView.RepaintAll();
                Repaint();
            }
        }
        Handles.matrix = prevHandlesMatrix;
    }
    
    void TickSceneViewInput()
    {
        Event e = Event.current;
        if (
            e.type == EventType.KeyDown 
            && e.keyCode == KeyCode.Keypad1 || (e.keyCode == KeyCode.Alpha1 && e.modifiers == EventModifiers.Alt)
            )// && e.modifiers == EventModifiers.Shift)
        {
            Debug.LogWarning("Pressed 1 key!");
            decorator.type = BoxBrushDecorationType.CORNER;
            decoratorDirtied = true;
            e.Use();
        }
        
        if (
            e.type == EventType.KeyDown 
            && e.keyCode == KeyCode.Keypad2 || (e.keyCode == KeyCode.Alpha2 && e.modifiers == EventModifiers.Alt)
            )
        {
            Debug.LogWarning("Pressed 2 key!");
            decorator.type = BoxBrushDecorationType.EDGE;
            decoratorDirtied = true;
            e.Use();
        }
        
        if (
            e.type == EventType.KeyDown 
            && e.keyCode == KeyCode.Keypad3 || (e.keyCode == KeyCode.Alpha3 && e.modifiers == EventModifiers.Alt)
            )
        {
            Debug.LogWarning("Pressed 3 key!");
            decorator.type = BoxBrushDecorationType.FACE;
            decoratorDirtied = true;
            e.Use();
        }

        if (decoratorDirtied)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(decorator);
            Repaint();
        }
    }

    void TickOnInspectorInput()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.M)
        {
            Debug.LogWarning("M KEYPRESS!");
            e.Use();
        }
    }
    //
    // void DrawGroups()
    // {
    //     if (decorator.type.HasFlag(BoxBrushDecorationType.EDGE))
    //     {
    //         using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
    //         {
    //             EditorGUILayout.LabelField("EDGE:");
    //             EditorGUILayout.Space();
    //         }
    //     }
    //
    //     if (decorator.type.HasFlag(BoxBrushDecorationType.FACE))
    //     {
    //         using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
    //         {
    //             EditorGUILayout.LabelField("FACE:");
    //             EditorGUILayout.Space();
    //         }
    //     }
    //
    //     if (decorator.type.HasFlag(BoxBrushDecorationType.CORNER))
    //     {
    //         using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
    //         {
    //             EditorGUILayout.LabelField("CORNER:");
    //             EditorGUILayout.Space();
    //         }
    //     }
    // }
}
