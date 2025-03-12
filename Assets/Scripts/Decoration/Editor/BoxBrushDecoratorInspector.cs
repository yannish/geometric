using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

[CustomEditor(typeof(BoxBrushDecorator))]
public class BoxBrushDecoratorInspector : Editor
{
    private SerializedProperty typeProp;
    
    private SerializedProperty cornerStatesProp;
    private SerializedProperty edgeStatesProp;
    private SerializedProperty faceStatesProp;
    
    private SerializedProperty cornerSettingsProp;
    private SerializedProperty edgeSettingsProp;
    private SerializedProperty faceSettingsProp;
    
    // private SerializedProperty dimsProp;
    private SerializedProperty debugProp;
    private SerializedProperty prefabProp;
    private SerializedProperty calculatedPrefabSizeProp;
    
    private BoxBrushDecorator decorator;
    private BoxColliderExtendedEditor boxColliderExtendedEditor;

    private int selectedCorner_OLD = -1;
    private bool decoratorDirtied = false;
    private bool changedPrefabAsset;
    private BoxBrushDecorationType prevDecoratorType;
    
    //... SELECTION:
    public BoxBrushDirection? selectedFace;
    public BoxBrushCornerType? selectedCorner;
    public BoxBrushEdge? selectedEdge;
    
    private const float k_groupSpacing = 6f;
    
    
    private void OnEnable()
    {
        decorator = target as BoxBrushDecorator;
        
        typeProp = serializedObject.FindProperty("type");

        faceStatesProp = serializedObject.FindProperty("faceStates");
        cornerStatesProp = serializedObject.FindProperty("cornerStates");
        edgeStatesProp = serializedObject.FindProperty("edgeStates");
        
        faceSettingsProp = serializedObject.FindProperty("faceSettings");
        cornerSettingsProp = serializedObject.FindProperty("cornerSettings");
        edgeSettingsProp = serializedObject.FindProperty("edgeSettings");
        // dimsProp = serializedObject.FindProperty("dimensions");
        
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
                Debug.LogWarning("Found boxColliderExtendedEditor");
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

        selectedCorner = null;
        selectedFace = null;
    }

    private void HandleBoxColliderEdit()
    {
        Debug.LogWarning("box collider was edited, reacting");
        UpdateDirtyDecorator();
    }


    public override void OnInspectorGUI()
    {
        decoratorDirtied = false;
        changedPrefabAsset = false;
        
        serializedObject.DrawScriptField();
        serializedObject.Update();
        
        EditorGUI.BeginChangeCheck();
        
        DrawConfigContent();
        
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("PRESETS:", EditorStyles.boldLabel);
            DrawPresetControls();
            // if (GUILayout.Button("SNAP LOWER BOUNDS TO PIVOT"))
            // {
            //     SnapLowerBoundsToPivot();
            // }
        }
        EditorGUILayout.Space(k_groupSpacing);

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("SETTINGS:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(debugProp);
            EditorGUILayout.PropertyField(cornerSettingsProp);
            EditorGUILayout.PropertyField(edgeSettingsProp);
            EditorGUILayout.PropertyField(faceSettingsProp);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(k_groupSpacing);
        
        

        
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("STATES:", EditorStyles.boldLabel);
            
            
            EditorGUI.indentLevel++;
            switch (decorator.type)
            {
                case BoxBrushDecorationType.FACE:
                    EditorGUILayout.PropertyField(faceStatesProp);
                    break;
                case BoxBrushDecorationType.CORNER:
                    EditorGUILayout.PropertyField(cornerStatesProp);
                    break;
                case BoxBrushDecorationType.EDGE:
                    EditorGUILayout.PropertyField(edgeStatesProp);
                    break;
            }
            EditorGUI.indentLevel--;
        }
        
        if (EditorGUI.EndChangeCheck())
        {
            Debug.Log("something changed in brush inspector.");
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            UpdateDirtyDecorator();
            EditorUtility.SetDirty(decorator);
        }

        if (decoratorDirtied)
        {
            decoratorDirtied = false;
        }
    }
    
    void DrawConfigContent()
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("CONFIG:", EditorStyles.boldLabel);
            
            prevDecoratorType = decorator.type;
            // EditorGUI.BeginChangeCheck();
            // EditorGUILayout.PropertyField(typeProp);
            // if (EditorGUI.EndChangeCheck())
            // {
            //     
            // }
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(prefabProp);
            if (EditorGUI.EndChangeCheck())
            {
                Debug.LogWarning("Changed prefab asset.");
                changedPrefabAsset = true;
            }

            if (decorator.prefab != null)
            {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(calculatedPrefabSizeProp);
                GUI.enabled = true;
            }
            
            // EditorGUILayout.PropertyField(typeProp);
            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("CORNER", EditorStyles.miniButtonLeft))
                {
                    decorator.type = BoxBrushDecorationType.CORNER;
                    // typeProp.enumValueIndex = (int)BoxBrushDecorationType.CORNER;
                }
                if (GUILayout.Button ("EDGE", EditorStyles.miniButtonMid)) 
                {
                    decorator.type  = BoxBrushDecorationType.EDGE;
                    // typeProp.enumValueIndex = (int)BoxBrushDecorationType.EDGE;
                }
                if (GUILayout.Button ("FACE", EditorStyles.miniButtonRight))
                {
                    decorator.type = BoxBrushDecorationType.FACE;
                    // typeProp.enumValueIndex = (int)BoxBrushDecorationType.FACE;
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Debug.Log("something changed in RADIO BUTTONS");
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                UpdateDirtyDecorator();
                EditorUtility.SetDirty(decorator);
            }
            // EditorGUILayout.PropertyField(dimsProp);
        }
        EditorGUILayout.Space(k_groupSpacing);
    }
    

    private void DrawPresetControls()
    {
        switch (decorator.type)
        {
            case BoxBrushDecorationType.FACE:
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("FULL"))
                    {
                        foreach(var face in decorator.faceStates)
                            face.isMuted = false;
                    }
                    if (GUILayout.Button("WALLS"))
                    {
                        foreach (var face in decorator.faceStates)
                        {
                            face.isMuted = (face.direction == BoxBrushDirection.UP || face.direction == BoxBrushDirection.DOWN);
                        }
                    }
                    if (GUILayout.Button("CEILING"))
                    {
                        foreach (var face in decorator.faceStates)
                        {
                            face.isMuted = face.direction != BoxBrushDirection.UP;
                        }
                    }
                    if (GUILayout.Button("FLOOR"))
                    {
                        foreach (var face in decorator.faceStates)
                        {
                            face.isMuted = face.direction != BoxBrushDirection.DOWN;
                        }
                    }
                    if (GUILayout.Button("CEILING & FLOOR"))
                    {
                        foreach (var face in decorator.faceStates)
                        {
                            face.isMuted = (face.direction != BoxBrushDirection.DOWN && face.direction != BoxBrushDirection.UP);
                        }
                    }
                }
                break;
            
            case BoxBrushDecorationType.CORNER:
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("FULL"))
                    {
                        foreach (var corner in decorator.cornerStates)
                            corner.isMuted = false;
                    }
                    if (GUILayout.Button("CEILING"))
                    {
                        foreach (var corner in decorator.cornerStates)
                        {
                            corner.isMuted =
                                corner.direction != BoxBrushCornerType.BACK_TOP_LEFT
                                && corner.direction != BoxBrushCornerType.BACK_TOP_RIGHT
                                && corner.direction != BoxBrushCornerType.FRONT_TOP_RIGHT
                                && corner.direction != BoxBrushCornerType.FRONT_TOP_LEFT;
                        }
                    }
                    if (GUILayout.Button("FLOOR"))
                    {
                        foreach (var corner in decorator.cornerStates)
                        {
                            corner.isMuted =
                                !(corner.direction != BoxBrushCornerType.BACK_TOP_LEFT
                                  && corner.direction != BoxBrushCornerType.BACK_TOP_RIGHT
                                  && corner.direction != BoxBrushCornerType.FRONT_TOP_RIGHT
                                  && corner.direction != BoxBrushCornerType.FRONT_TOP_LEFT);
                        }
                    }
                }
                break;
            
            case BoxBrushDecorationType.EDGE:
                break;
        }
    }

    private void SnapLowerBoundsToPivot()
    {
        // if (decorator.BoxCollider == null)
        //     return;
        //
        // var pivotPosLocal = decorator.transform.position
        // var centerToPivot = decorator.BoxCollider.center.To
        // decorator.BoxCollider.
    }

    public void UpdateDirtyDecorator()
    {
        //... CORNERS:
        if (
            changedPrefabAsset
            || (prevDecoratorType == BoxBrushDecorationType.CORNER && decorator.type != BoxBrushDecorationType.CORNER)
            )
        {
            Debug.LogWarning("CLEARING CORNERS!");
            decorator.RecalculateCorners();
            decorator.ClearCorners();
        }
        
        if (prevDecoratorType != BoxBrushDecorationType.CORNER && decorator.type == BoxBrushDecorationType.CORNER)
        {
            decorator.RecalculateCorners();
            decorator.UpdateCorners();
        }
        
        if (decorator.type == BoxBrushDecorationType.CORNER)
        {
            Debug.LogWarning("Updating corners.");
            decorator.RecalculateCorners();
            decorator.UpdateCorners();
        }
        
        //... switched off Faces
        if (
            changedPrefabAsset
            || (prevDecoratorType == BoxBrushDecorationType.FACE && decorator.type != BoxBrushDecorationType.FACE)
            )
        {
            Debug.LogWarning("CLEARING FACES");
            foreach (var face in decorator.faceStates)
            {
                BoxBrushDecoratorExtensions.RecalculateDecoratorFace(decorator, face);
                BoxBrushDecoratorExtensions.ClearDecoratorFace(decorator, face);
            }
        }


        //... switched to Faces
        if (prevDecoratorType != BoxBrushDecorationType.FACE && decorator.type == BoxBrushDecorationType.FACE)
        {
            foreach (var face in decorator.faceStates)
            {
                if (face.isMuted)
                {
                    BoxBrushDecoratorExtensions.RecalculateDecoratorFace(decorator, face);
                    BoxBrushDecoratorExtensions.ClearDecoratorFace(decorator, face);
                    continue;
                }

                bool instanceCountChange = BoxBrushDecoratorExtensions.RecalculateDecoratorFace(decorator, face);
            
                if (
                    instanceCountChange
                    || face.instances == null
                    || face.instances.Count != face.positions.Count
                )
                {
                    BoxBrushDecoratorExtensions.ClearDecoratorFace(decorator, face);
                    BoxBrushDecoratorExtensions.RegenerateFaceInstances(decorator, face);
                }
            
                BoxBrushDecoratorExtensions.RealignDecoratorFaceInstances(decorator, face);
            }
        }
        
        //... just update Faces
        if (decorator.type == BoxBrushDecorationType.FACE)
        {
            foreach (var face in decorator.faceStates)
            {
                if (face.isMuted)
                {
                    BoxBrushDecoratorExtensions.RecalculateDecoratorFace(decorator, face);
                    BoxBrushDecoratorExtensions.ClearDecoratorFace(decorator, face);
                    continue;
                }

                bool instanceCountChange = BoxBrushDecoratorExtensions.RecalculateDecoratorFace(decorator, face);
                
                if (
                    instanceCountChange
                    || face.instances == null
                    || face.instances.Count != face.positions.Count
                )
                {
                    BoxBrushDecoratorExtensions.ClearDecoratorFace(decorator, face);
                    BoxBrushDecoratorExtensions.RegenerateFaceInstances(decorator, face);
                }
                
                BoxBrushDecoratorExtensions.RealignDecoratorFaceInstances(decorator, face);
            }
        }
    }

    void UpdateCorners()
    {
        
    }
    
    void UpdateFaces()
    {
        
    }

    void UpdateEdges()
    {
        
    }
    
    public void OnSceneGUI()
    {
        TickOnInspectorInput();
        TickSceneViewInput();   

        DrawCornerSelectionControls();
        // DrawCornerControls();
        DrawFaceControls();
        DrawEdgeControls();
    }

    
    private void DrawEdgeControls()
    {
        if (decorator.type != BoxBrushDecorationType.EDGE)
            return;
    }

    private void DrawFaceControls()
    {
        if (decorator.type != BoxBrushDecorationType.FACE)
            return;
        
        foreach (var kvp in BoxBrushDirections.faceDirLookup)
        {
            var dir = kvp.Value;
            var handlePos = Vector3.Scale(dir, decorator.halfDims);
            
            var tangent = BoxBrushDirections.tangentLookup[kvp.Key];
            var bitangent = BoxBrushDirections.bitangentLookup[kvp.Key];
            var handleRot = Quaternion.LookRotation(-dir, tangent);
            
            // var handleRot = Quaternion.LookRotation(dir, Vector3.up);
            // int controlId = GUIUtility.GetControlID(FocusType.Passive);
            
            float effectiveShowSize = selectedFace != null && kvp.Key == selectedFace
                ? decorator.debug.elementPickSize
                : decorator.debug.elementSelectedInflation;

            var hotControl = GUIUtility.hotControl;
            
            using (new Handles.DrawingScope(
                       ColorPick.Swatches.boxBrushSubHandles, 
                       decorator.transform.localToWorldMatrix
                   ))
            {
                if (Handles.Button(
                        handlePos, 
                        handleRot,
                        effectiveShowSize,
                        decorator.debug.elementPickSize,
                        Handles.RectangleHandleCap
                    ))
                {
                    Debug.LogWarning($"Selected corner: {kvp.Key}");
                    
                    selectedFace = kvp.Key;
                    selectedCorner = null;
                    
                    SceneView.RepaintAll();
                    Repaint();
                }
            }

            if (hotControl != 0)
            {
                // Debug.LogWarning("... found a spicy hotcontrol.");
                SceneView.RepaintAll();
            }
        }
    }

    void DrawCornerSelectionControls()
    {
        if (decorator.type != BoxBrushDecorationType.CORNER)
            return;
        
        var camForward = SceneView.currentDrawingSceneView.camera.transform.forward;
        var camUp = SceneView.currentDrawingSceneView.camera.transform.up;

        foreach (var kvp in BoxBrushDirections.cornerNormalLookup)
        {
            var dir = kvp.Value;
            var handlePos = Vector3.Scale(dir, decorator.halfDims);
            var handleRot = Quaternion.LookRotation(camForward, camUp);
            // var handleRot = Quaternion.LookRotation(dir, Vector3.up);
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            float effectiveShowSize = selectedCorner != null && kvp.Key == selectedCorner
                ? decorator.debug.elementPickSize
                : decorator.debug.elementSelectedInflation;

            using (new Handles.DrawingScope(
                       ColorPick.Swatches.boxBrushSubHandles, 
                       decorator.transform.localToWorldMatrix
                       ))
            {
                if (Handles.Button(
                        handlePos, 
                        handleRot,
                        effectiveShowSize,
                        decorator.debug.elementPickSize,
                        Handles.RectangleHandleCap
                    ))
                {
                    Debug.LogWarning($"Selected corner: {kvp.Key}");
                    selectedCorner = kvp.Key;
                    selectedFace = null;
                    
                    SceneView.RepaintAll();
                    Repaint();
                }
            }
        }
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
            effectiveHandleSize *= (int)kvp.Key == selectedCorner_OLD ? decorator.debug.elementSelectedInflation : 1f;
            
            if (Handles.Button(
                    handlePos,
                    handleRot,
                    effectiveHandleSize,
                    decorator.debug.elementPickSize,
                    Handles.RectangleHandleCap
                ))
            {
                selectedCorner_OLD = (int)kvp.Key;
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
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
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
    
}
