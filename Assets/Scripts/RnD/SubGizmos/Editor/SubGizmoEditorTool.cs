using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

[EditorTool("SubGizmo Tool", typeof(SubGizmoMono))]
public class SubGizmoEditorTool : BaseEditorTool
{
    static Rect s_ToolSettingsWindow = new Rect(10f, 30f, 256f, 205f);
    
    SubGizmoMono subGizmoMono;
    
    SubGizmoMonoEditor subGizmoMonoEditor;

    private SerializedProperty arrayProp;
    
    private SerializedObject serializedObject;
    
    public SubGizmoEditorTool()
    {
        displayName = "";
        // displayName = "SubGizmo Tool";
        tooltip = "Play around with selecting subelements of a single component.";
    }
    
    protected override string iconName => "Subgizmo.png";

    public override void DrawHandles()
    {

    }

    public override void OnActivated()
    {
        Debug.LogWarning("SubGizmo tool activated.");
        subGizmoMono = target as SubGizmoMono;
        serializedObject = new SerializedObject(subGizmoMono);
        arrayProp = serializedObject.FindProperty("dataArray");
        if(arrayProp != null)
            Debug.LogWarning("found data array");
        
        ActiveEditorTracker editorTracker = ActiveEditorTracker.sharedTracker;
        Editor[] editors = editorTracker.activeEditors;

        foreach (var editor in editors)
        {
            if (editor.target is SubGizmoMono)
            {
                Debug.LogWarning("Found subgizmo editor.");
                subGizmoMonoEditor = editor as SubGizmoMonoEditor;
                break;
            }
        }
    }

    public override void OnWillBeDeactivated()
    {
        Debug.LogWarning("SubGizmo tool deactivated.");
    }
    
    void InSceneWindow(int windowId)
    {
        var previousWideMode = EditorGUIUtility.wideMode;
        var previousLabelWidth = EditorGUIUtility.labelWidth;
        
        EditorGUIUtility.wideMode = true;
        EditorGUIUtility.labelWidth = 85f;

        var rect = new Rect(0f, 0f, s_ToolSettingsWindow.width, 50f);
        GUI.DragWindow(rect);
        
        using (var checkScope = new EditorGUI.ChangeCheckScope())
        {
            // using(var disabledScope = new EditorGUI.DisabledGroupScope(m_HandleMode == HandleMode.AutoSmooth))
            
            using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                subGizmoMono.subGizmoType = (SubGizmoType)EditorGUILayout.EnumPopup("SubGizmo Type", subGizmoMono.subGizmoType);
                if (subGizmoMono.subGizmoType == SubGizmoType.FACE)
                {
                    EditorGUILayout.LabelField("... get off of Face mode to touch QuickFloat");//, EditorStyles.boldLabel);
                }
                else
                {
                    subGizmoMono.quickFloat = EditorGUILayout.FloatField(subGizmoMono.quickFloat);
                }
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (subGizmoMonoEditor.selectedSubgizmoCorner != null)
                {
                    EditorGUILayout.LabelField(
                        $"Selected corner: {subGizmoMonoEditor.selectedSubgizmoCorner}, " +
                        $"int value: {(int)subGizmoMonoEditor.selectedSubgizmoCorner}"
                        );
                    if (arrayProp != null)
                    {
                        var arrayElement = arrayProp.GetArrayElementAtIndex((int)subGizmoMonoEditor.selectedSubgizmoCorner - 5);
                        DrawSubGizmoData(arrayElement);
                    }
                    // SerializedProperty
                }

                if (subGizmoMonoEditor.selectedSubgizmoFace != null)
                {
                    EditorGUILayout.LabelField($"Selected face: {subGizmoMonoEditor.selectedSubgizmoFace}");
                    // if (arrayProp != null)
                    // {
                    //     var arrayElement = arrayProp.GetArrayElementAtIndex((int)subGizmoMonoEditor.selectedSubgizmoFace);
                    //     DrawSubGizmoData(arrayElement);
                    // }
                }
            }

            if (checkScope.changed)
            {
                Debug.LogWarning("made a change to subGizmo through its tool.");
                EditorUtility.SetDirty(subGizmoMono);
            }
        }
        
        EditorGUIUtility.wideMode = previousWideMode;
        EditorGUIUtility.labelWidth = previousLabelWidth;
    }

    public override void OnToolGUI(EditorWindow window)
    {
        s_ToolSettingsWindow = GUI.Window(42, s_ToolSettingsWindow, InSceneWindow, "DECORATION ELEMENT");
        // base.OnToolGUI(window);
    }

    void DrawSubGizmoData(SerializedProperty prop)
    {
        SerializedProperty boolProp = prop.FindPropertyRelative("quickBool");
        SerializedProperty floatProp = prop.FindPropertyRelative("quickFloat");
        SerializedProperty stringProp = prop.FindPropertyRelative("quickString");
        EditorGUILayout.PropertyField(boolProp);
        EditorGUILayout.PropertyField(floatProp);
        EditorGUILayout.PropertyField(stringProp);
    }

    [UnityEditor.ShortcutManagement.Shortcut("SubGizmo Tool", null, KeyCode.Q, ShortcutModifiers.Alt)]
    static void ToolShortcut()
    {
        if(Selection.GetFiltered<SubGizmoMono>(SelectionMode.TopLevel).Length > 0)
            ToolManager.SetActiveTool<SubGizmoEditorTool>();
    }
}
