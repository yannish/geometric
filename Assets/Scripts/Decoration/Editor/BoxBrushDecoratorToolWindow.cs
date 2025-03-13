using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

public class BoxBrushDecoratorToolWindow : EditorWindow
{
    private void OnDisable()
    {
        if (ToolManager.activeToolType == typeof(BoxBrushDecoratorTool))
        {
            ToolManager.RestorePreviousPersistentTool();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("IT's A NEW WINDOW!");
    }
}
