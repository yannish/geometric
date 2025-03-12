using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

[EditorTool("Decorator Tool", typeof(BoxBrushDecorator))]
public class BoxBrushDecoratorTool : BaseEditorTool
{
    static Rect editElementWindowRect = new Rect(80f, 30f, 256f, 205f);
    
    private BoxBrushDecorator decorator;
    private BoxBrushDecoratorInspector decoratorInspector;
    
    private SerializedObject serializedObject;
    private SerializedProperty facesProp;
    private SerializedProperty cornersProp;
    private SerializedProperty edgesProp;
    
    public BoxBrushDecoratorTool()
    {
        displayName = "Decorator Tool";
        tooltip = "Switch modes:\n\n" +
                  "  - CORNER (ALT + 1)\n\n" +
                  "  - EDGE (ALT + 2)\n\n" +
                  "  - FACE (ALT + 3)\n\n" +
                  "\n" +
                  "Click an element in scene view to edit it.";
    }
    
    protected override string iconName => "DecoratorIcon.png";


    public override void OnActivated()
    {
        Debug.LogWarning("Decorator Tool activated");
        decorator = target as BoxBrushDecorator;
        serializedObject = new SerializedObject(decorator);
        
        facesProp = serializedObject.FindProperty("faceStates");
        cornersProp = serializedObject.FindProperty("cornerStates");
        // edgesProp = serializedObject.FindProperty("edgeStates");
        
        ActiveEditorTracker editorTracker = ActiveEditorTracker.sharedTracker;
        Editor[] editors = editorTracker.activeEditors;
        foreach (var editor in editors)
        {
            if (editor.target is BoxBrushDecorator)
            {
                Debug.LogWarning("Found decorator inspector.");
                decoratorInspector = editor as BoxBrushDecoratorInspector;
                break;
            }
        }
    }

    public override void OnWillBeDeactivated()
    {
        
    }
    
    public override void DrawHandles()
    {
        Handles.DrawWireDisc(decorator.transform.position, Vector3.up, 2f);
    }

    void InSceneViewWindow(int windowID)
    {
        serializedObject.Update();
        
        var previousWideMode = EditorGUIUtility.wideMode;
        var previousLabelWidth = EditorGUIUtility.labelWidth;
        
        EditorGUIUtility.wideMode = true;
        EditorGUIUtility.labelWidth = 85f;

        var rect = new Rect(0f, 0f, editElementWindowRect.width, 50f);
        GUI.DragWindow(rect);

        using (var checkScope = new EditorGUI.ChangeCheckScope())
        {
            switch (decorator.type)
            {
                case BoxBrushDecorationType.FACE:
                    // EditorGUILayout.LabelField("Faces");
                    if (decoratorInspector.selectedFace == null)
                    {
                        EditorGUILayout.LabelField("Select a FACE to edit it.");
                    }
                    else
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField($"FACE: ", EditorStyles.boldLabel, GUILayout.Width(40f));
                            GUIStyle rightAlignStyle = new GUIStyle(EditorStyles.label)
                            {
                                alignment = TextAnchor.MiddleRight
                            };
                            EditorGUILayout.LabelField($" {decoratorInspector.selectedFace}", rightAlignStyle);
                        }
                        if (facesProp != null && facesProp.arraySize > 0)
                        {
                            int index = (int)decoratorInspector.selectedFace;
                            DrawFaceElementData(facesProp.GetArrayElementAtIndex(index));
                        }
                    }
                    break;
                
                case BoxBrushDecorationType.CORNER:
                    EditorGUILayout.LabelField("Corners");
                    if (decoratorInspector.selectedCorner == null)
                    {
                        EditorGUILayout.LabelField("Select a CORNER to edit it.");
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Select a CORNER to edit it.");
                    }
                    break;
                
                case BoxBrushDecorationType.EDGE:
                    EditorGUILayout.LabelField("Edges");
                    // if (decoratorInspector.selectedEdge == null)
                    // {
                    //     EditorGUILayout.LabelField("Select a CORNER to edit it.");
                    // }
                    break;
            }

            if (checkScope.changed)
            {
                Debug.LogWarning("made a change to a decorator element through its tool.");
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(decorator);
                SceneView.RepaintAll();
            }
        }
        
        EditorGUIUtility.wideMode = previousWideMode;
        EditorGUIUtility.labelWidth = previousLabelWidth;
    }

    public override void OnToolGUI(EditorWindow window)
    {
        editElementWindowRect = GUI.Window(42, editElementWindowRect, InSceneViewWindow, "DECORATOR ELEMENT");
    }

    void DrawCornerEditing()
    {
        
    }

    void DrawFaceEditing()
    {
        
    }
    
    void DrawFaceElementData(SerializedProperty prop)
    {
        prop.NextVisible(true);
        while (prop.NextVisible(false))
        {
            EditorGUILayout.PropertyField(prop, true);
        }
        
        // EditorGUILayout.PropertyField(prop);
        
        // var props = prop.GetEnumerator();
        // while (props.MoveNext())
    }
    
    void DrawCornerElementData(SerializedProperty prop)
    {
        
    }
    
    void DrawEdgeElementData(SerializedProperty prop)
    {
        
    }

    [Shortcut("Decorator Tool", null, KeyCode.E, ShortcutModifiers.Alt)]
    static void ToolShortcut()
    {
        Debug.LogWarning("Decorator Tool shortcut");
        if(Selection.GetFiltered<BoxBrushDecorator>(SelectionMode.TopLevel).Length > 0)
            ToolManager.SetActiveTool<BoxBrushDecoratorTool>();
    }
}
