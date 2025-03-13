using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

[EditorTool("Decorator Tool", typeof(BoxBrushDecorator))]
public class BoxBrushDecoratorTool : BaseEditorTool
{
    static Rect editElementWindowRect = new Rect(80f, 30f, 300f, 256f);
    
    private BoxBrushDecorator decorator;
    private BoxBrushDecoratorInspector decoratorInspector;
    private BoxBrushDecoratorToolWindow window;
    
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
        
        // window = EditorWindow.GetWindow<BoxBrushDecoratorToolWindow>();
    }

    public override void OnWillBeDeactivated()
    {
        // window.Close();
    }

    public void OnDisable()
    {
        if(window != null)
            window.Close();
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

        var rect = new Rect(0f, 0f, editElementWindowRect.width, 24f);
        // GUI.DragWindow(rect);

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
                        GUILayout.Space(24f);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUIStyle rightAlignStyle = new GUIStyle(EditorStyles.boldLabel)
                            {
                                alignment = TextAnchor.MiddleRight
                            };
                            EditorGUILayout.LabelField($"FACE: ", rightAlignStyle);//, GUILayout.Width(40f));
                            GUIStyle leftAlignStyle = new GUIStyle(EditorStyles.label)
                            {
                                alignment = TextAnchor.MiddleLeft
                            };
                            EditorGUILayout.LabelField($" {decoratorInspector.selectedFace}", leftAlignStyle);
                        }
                        GUILayout.Space(24f);
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
                        GUILayout.Space(24f);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUIStyle rightAlignStyle = new GUIStyle(EditorStyles.boldLabel)
                            {
                                alignment = TextAnchor.MiddleRight
                            };
                            EditorGUILayout.LabelField($"CORNER: ", rightAlignStyle);//, GUILayout.Width(60f));
                            GUIStyle leftAlignStyle = new GUIStyle(EditorStyles.label)
                            {
                                alignment = TextAnchor.MiddleLeft
                            };
                            EditorGUILayout.LabelField($" {decoratorInspector.selectedCorner}", leftAlignStyle);
                        }
                        GUILayout.Space(24f);
                        if (facesProp != null && facesProp.arraySize > 0)
                        {
                            int index = (int)decoratorInspector.selectedCorner;
                            DrawCornerElementData(cornersProp.GetArrayElementAtIndex(index));
                        }
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
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                decoratorInspector.UpdateDirtyDecorator();
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
        EditorGUILayout.PropertyField(prop.FindPropertyRelative("isMuted"));
        EditorGUILayout.PropertyField(prop.FindPropertyRelative("orientation"));
        
        var overrideFillProp = prop.FindPropertyRelative("overrideFill");
        EditorGUILayout.PropertyField(overrideFillProp);
        if (overrideFillProp.boolValue)
        {
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("fill"));
        }
        
        var overrideInstanceCountProp = prop.FindPropertyRelative("overrideInstanceCount");
        EditorGUILayout.PropertyField(overrideInstanceCountProp);
        if (overrideInstanceCountProp.boolValue)
        {
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("numInstances"));
        }
        
        var overridePaddingProp = prop.FindPropertyRelative("overridePadding");
        EditorGUILayout.PropertyField(overridePaddingProp);
        if (overridePaddingProp.boolValue)
        {
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("padding"));
        }
        
        var overrideSpacingProp = prop.FindPropertyRelative("overrideSpacing");
        EditorGUILayout.PropertyField(overrideSpacingProp);
        if (overrideSpacingProp.boolValue)
        {
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("spacing"));
        }
        
        // EditorGUILayout.PropertyField(prop.FindPropertyRelative("isMuted"));
        // EditorGUILayout.PropertyField(prop.FindPropertyRelative("isMuted"));

        // prop.NextVisible(true);
        // while (prop.NextVisible(false))
        // {
        //     EditorGUILayout.PropertyField(prop);
        // }
        
        // EditorGUILayout.PropertyField(prop);
        
        // var props = prop.GetEnumerator();
        // while (props.MoveNext())
    }
    
    void DrawCornerElementData(SerializedProperty prop)
    {
        EditorGUILayout.PropertyField(prop.FindPropertyRelative("isMuted"));
        
        var overrideInsetAmountProp = prop.FindPropertyRelative("overrideInsetAmount");
        EditorGUILayout.PropertyField(overrideInsetAmountProp);
        if (overrideInsetAmountProp.boolValue)
        {
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("insetAmount"));
        }
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
