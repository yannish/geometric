using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

[CustomEditor(typeof(SubGizmoMono))]
public class SubGizmoMonoEditor : Editor
{
    Dictionary<int, string> handleLookup = new Dictionary<int, string>();

    public int selectedHandle = -1;
    public SubGizmoCorner? selectedSubgizmoCorner;
    public SubGizmoDirection? selectedSubgizmoFace;

    private SerializedProperty quickDatasProp;
    private SerializedProperty typeProp;
    private SerializedProperty dataArrayProp;

    private void OnEnable()
    {
        quickDatasProp = serializedObject.FindProperty("quickDatas");
        typeProp = serializedObject.FindProperty("subGizmoType");
        dataArrayProp = serializedObject.FindProperty("dataArray");
    }

    private void OnDisable()
    {
        selectedSubgizmoCorner = null;
        selectedSubgizmoFace = null;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.DrawScriptField();
        
        var subGizmo = target as SubGizmoMono;
        
        serializedObject.Update();
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(typeProp);
        EditorGUILayout.PropertyField(quickDatasProp);
        EditorGUILayout.PropertyField(dataArrayProp);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            // Debug.LogWarning("change detected");
            foreach (var data in subGizmo.quickDatas)
            {
                var prevPosCount = data.calculatedPositions.Count;
                data.calculatedFloat = data.shownFloat * 0.5f;
                var roundedDown = Mathf.FloorToInt(data.calculatedFloat);
                if (roundedDown != prevPosCount)
                {
                    Debug.LogWarning("change in position count!");
                    data.calculatedPositions.Clear();
                    for (int i = 0; i < roundedDown; i++)
                    {
                        var newPos = Vector3.right * i;
                        data.calculatedPositions.Add(newPos);
                    }
                }
            }
            
            Repaint();
        }
        
        // if (serializedObject.ApplyModifiedProperties())
        // {
        // }
    }

    void DrawSelectionFlow()
    {
        if (selectedHandle == -1)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Select a face, edge, or corner to edit them.");
            }
        }
        
        DrawSelectedElement();
    }

    void DrawSelectedElement()
    {
        if (selectedHandle <= 4)
        {
            SubGizmoDirection gizmoDir = (SubGizmoDirection)selectedHandle;
            switch (gizmoDir)
            {
                case SubGizmoDirection.FORWARD:
                case SubGizmoDirection.BACKWARD:
                case SubGizmoDirection.LEFT:
                case SubGizmoDirection.RIGHT:
                    DrawFaceDetails();
                    break;
            }
        }
        else
        {
            SubGizmoCorner gizmoCorner = (SubGizmoCorner)selectedHandle;
            switch (gizmoCorner)
            {
                case SubGizmoCorner.FRONT_TOP_LEFT:
                case SubGizmoCorner.FRONT_TOP_RIGHT:
                case SubGizmoCorner.FRONT_BOTTON_LEFT:
                case SubGizmoCorner.FRONT_BOTTON_RIGHT:
                case SubGizmoCorner.BACK_TOP_LEFT:
                case SubGizmoCorner.BACK_TOP_RIGHT:
                case SubGizmoCorner.BACK_BOTTOM_LEFT:
                case SubGizmoCorner.BACK_BOTTOM_RIGHT:
                    DrawCornerDetails();
                    break;
            }
        }
    }

    void DrawFaceDetails()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Here's some stuff about the face you clicked.");
            EditorGUILayout.Space(10f);
        }
    }

    void DrawCornerDetails()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Here's some stuff about the corner you clicked.");
            EditorGUILayout.Space(10f);
        }
    }
    
    private void OnSceneGUI()
    {
        var subGizmo = (SubGizmoMono)target;

        Handles.DrawWireDisc(subGizmo.transform.position, Vector3.up, 1f);

        handleLookup.Clear();

        foreach (var kvp in SubGizmoDirections.lookup)
        {
            var dir = kvp.Value;

            switch (subGizmo.subGizmoType)
            {
                case SubGizmoType.FACE:
                    if (kvp.Key >= 5)
                        continue;
                    break;
                
                // case SubGizmoType.EDGE:
                //     break;
                
                case SubGizmoType.CORNER:
                    if (kvp.Key < 5)
                        continue;
                    break;
            }
            
            // if(kvp.Key)

            var handlePos = dir * subGizmo.size;
            // var handlePos = subGizmo.transform.position + dir * subGizmo.size;
            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            if (!handleLookup.ContainsKey(controlId))
                handleLookup.Add(controlId, dir.ToString());

            float effectiveShowSize = subGizmo.grabSize;
            effectiveShowSize *= kvp.Key == selectedHandle ? 1.3f : 1f;
            
            // var handleCap = Handles.SphereHandleCap()
            
            var rot = Quaternion.LookRotation(dir, Vector3.up);
            
            var prevHandlesMatrix = Handles.matrix;
            Handles.matrix = subGizmo.transform.localToWorldMatrix;
            if (
                Handles.Button(
                    handlePos,
                    rot,
                    effectiveShowSize,
                    subGizmo.grabSize,
                    Handles.RectangleHandleCap
                    ))
            {
                // obj.selectedHandleIndex = i;
                if (kvp.Key <= 4)
                {
                    Debug.Log($"Selected Handle: {(SubGizmoDirection)kvp.Key}");
                    selectedSubgizmoFace = (SubGizmoDirection)kvp.Key;
                    selectedSubgizmoCorner = null;
                }
                else
                {
                    Debug.Log($"Selected Handle: {(SubGizmoCorner)kvp.Key}");
                    selectedSubgizmoCorner = (SubGizmoCorner)kvp.Key;
                    selectedSubgizmoFace = null;
                }
                
                selectedHandle = kvp.Key;
                
                SceneView.RepaintAll();
                
                Repaint();
            }

            Handles.matrix = prevHandlesMatrix;

            // Handles.CubeHandleCap(controlId, handlePos, Quaternion.identity, subGizmo.grabSize, EventType.Repaint);
        }

        // var e = Event.current;
        // var nearestControl = HandleUtility.nearestControl;
        //
        // if (e.type == EventType.MouseDown)
        // {
        //     Debug.LogWarning($"Mouse down, nearest control {nearestControl}");
        //
        //     foreach (var kvp in handleLookup)
        //     {
        //         Debug.LogWarning($"{kvp.Key}: {kvp.Value}");
        //     }
        //     
        //     if(handleLookup.TryGetValue(nearestControl, out var value))
        //     {
        //         Debug.LogWarning($"clicked {value}");
        //     }
        //     
        //     e.Use();
        // }

        // if (e.type == EventType.MouseDown && 
    }
}