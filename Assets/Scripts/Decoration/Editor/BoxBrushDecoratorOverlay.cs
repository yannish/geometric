using System;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[InitializeOnLoad]
[Overlay(typeof(SceneView), "Decoration Element Editor", true)]
[Icon("Assets/Icons/ToolIcons/DecoratorIcon.png")]
public class BoxBrushDecoratorOverlay : Overlay, ITransientOverlay
{
    public BoxBrushDecoratorOverlay()
    {
        displayName = "Decoration Element Editor";
        
        // tool = "Switch modes:\n\n" +
        //           "  - CORNER (ALT + 1)\n\n" +
        //           "  - EDGE (ALT + 2)\n\n" +
        //           "  - FACE (ALT + 3)\n\n" +
        //           "\n" +
        //           "Click an element in scene view to edit it.";
    }
    
    private BoxBrushDecoratorInspector decoratorInspector;
    private BoxBrushDecorator decorator;
    private SerializedObject serializedObject;
    
    private SerializedProperty typeProp;
    private SerializedProperty quickFloatProp;

    private SerializedProperty selectedFaceProp;

    private Label edgeLabel;
    private Label cornerLabel;
    private Label faceLabel;

    
    public override void OnCreated()
    {
        Debug.Log("BoxBrushDecoratorOverlay.OnCreated");
    }

    public override void OnWillBeDestroyed()
    {
        Debug.Log("BoxBrushDecoratorOverlay.OnWillBeDestroyed");
    }

    public override VisualElement CreatePanelContent()
    {
        Debug.Log("BoxBrushDecoratorOverlay.CreatePanelContent");
        
        ActiveEditorTracker editorTracker = ActiveEditorTracker.sharedTracker;
        Editor[] editors = editorTracker.activeEditors;
        foreach (var editor in editors)
        {
            if (editor.target is BoxBrushDecorator)
            {
                Debug.LogWarning("Found decorator inspector.");
                decorator = editor.target as BoxBrushDecorator;
                decoratorInspector = editor as BoxBrushDecoratorInspector;
                serializedObject = new SerializedObject(editor.target);
                typeProp = serializedObject.FindProperty("type");
                quickFloatProp = serializedObject.FindProperty("quickFloat");
                selectedFaceProp = serializedObject.FindProperty("selectedFace");
                break;
            }
        }
        
        var root = new VisualElement();
        
        root.style.width = 300;
        root.style.height = 100;

        root.style.backgroundColor = new Color(0, 0, 0, 0.25f); // Semi-transparent background
        root.style.borderTopLeftRadius = 10;
        root.style.borderBottomRightRadius = 10;
        root.style.paddingLeft = 10;
        root.style.paddingTop = 5;
        
        // root.Add(new Label("DECORATOR OVERLAY STUFF"));
        
        faceLabel = new Label("FACE MODE");
        edgeLabel = new Label("EDGE MODE");
        cornerLabel = new Label("CORNER MODE");
        
        root.Add(faceLabel);
        root.Add(edgeLabel);
        root.Add(cornerLabel);
        
        PropertyField positionField = new PropertyField(typeProp);
        positionField.Bind(serializedObject); // Auto-update when edited
        positionField.RegisterValueChangeCallback(evt =>
        {
            UpdateLabelVisibility();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            decoratorInspector.UpdateDirtyDecorator();
            EditorUtility.SetDirty(decorator);
        });
        
        PropertyField quickFloatField = new PropertyField(quickFloatProp);
        quickFloatField.Bind(serializedObject);
        
        PropertyField selectedFaceField = new PropertyField(selectedFaceProp);
        selectedFaceField.Bind(serializedObject);
        selectedFaceField.RegisterValueChangeCallback(evt =>
        {
            Debug.LogWarning($"selected face is now : {selectedFaceProp.intValue}");
        });
        
        root.Add(positionField);
        root.Add(quickFloatField);

        return root;
    }

    private void UpdateLabelVisibility()
    {
        if (typeProp == null)
            return;
        
        switch((BoxBrushDecorationType) typeProp.intValue)
        {
            case BoxBrushDecorationType.FACE:
                faceLabel.style.display = DisplayStyle.Flex;
                edgeLabel.style.display = DisplayStyle.None;
                cornerLabel.style.display = DisplayStyle.None;
                break;
            case BoxBrushDecorationType.CORNER:
                faceLabel.style.display = DisplayStyle.None;
                edgeLabel.style.display = DisplayStyle.None;
                cornerLabel.style.display = DisplayStyle.Flex;
                break;
            case BoxBrushDecorationType.EDGE:
                faceLabel.style.display = DisplayStyle.None;
                edgeLabel.style.display = DisplayStyle.Flex;
                cornerLabel.style.display = DisplayStyle.None;
                break;
        }
    }

    public bool visible
    {
        get
        {
            if(Selection.activeGameObject != null)
                return Selection.activeGameObject.GetComponent<BoxBrushDecorator>() != null;
            return false;
        }
    }
}
