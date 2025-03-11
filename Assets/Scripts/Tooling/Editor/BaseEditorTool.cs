using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

public class BaseEditorTool : EditorTool
{
    private const string iconPath = "Assets/Project/Editor/Tools/ToolIcons/";
    
    internal virtual string iconName { get; }
    
    protected virtual void FetchIcon() => m_ToolIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath + iconName);
    
#pragma warning disable CS0649
    [SerializeField]
    Texture2D m_ToolIcon;
#pragma warning restore CS0649
    
    
}
