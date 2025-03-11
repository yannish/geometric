using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorTool("SubGizmo Tool", typeof(SubGizmoMono))]
public class SubGizmoEditorTool : BaseEditorTool
{
    static Rect s_ToolSettingsWindow = new Rect(10f, 30f, 256f, 105f);
    
    SubGizmoMono subGizmoMono;
    
    SubGizmoMonoEditor subGizmoMonoEditor;
    
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

        var rect = new Rect(1000f, 1000f, s_ToolSettingsWindow.width, 24f);
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

    [UnityEditor.ShortcutManagement.Shortcut("SubGizmo Tool", null, KeyCode.Q)]
    static void ToolShortcut()
    {
        if(Selection.GetFiltered<SubGizmoMono>(SelectionMode.TopLevel).Length > 0)
            ToolManager.SetActiveTool<SubGizmoEditorTool>();
    }
}
