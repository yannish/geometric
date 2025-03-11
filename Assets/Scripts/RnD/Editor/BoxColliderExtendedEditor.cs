using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoxCollider))]
public class BoxColliderExtendedEditor : Editor
{
    // private Editor _internalEditor;

    private BoxCollider boxCollider;

    private Vector3 prevCenter;
    private Vector3 prevSize;
    
    public Action OnBoxColliderChanged;

    
    private void OnEnable()
    {
        boxCollider = target as BoxCollider;

        // Type editorType = Type.GetType("UnityEditor.BoxColliderEditor, UnityEditor");
        // if(editorType != null)
        //     _internalEditor = CreateEditor(targets, editorType);

        EditorApplication.update -= ChangeCheck;
        EditorApplication.update += ChangeCheck;
    }

    private void OnDisable()
    {
        EditorApplication.update -= ChangeCheck;
        // if(_internalEditor != null)
        //     DestroyImmediate(_internalEditor);
    }

    void OnDestroy()
    {
        // if(_internalEditor != null)
        //     DestroyImmediate(_internalEditor);
    }
    
    private void ChangeCheck()
    {
        if (boxCollider == null)
            return;

        if (prevCenter != boxCollider.center || prevSize != boxCollider.size)
        {
            OnBoxColliderChanged?.Invoke();
        }
        
        prevCenter = boxCollider.center;
        prevSize = boxCollider.size;
    }

    // public override void OnInspectorGUI()
    // {
    //     // EditorGUI.BeginChangeCheck();
    //     // if(_internalEditor != null)
    //     //     _internalEditor.OnInspectorGUI();
    //     // if (EditorGUI.EndChangeCheck())
    //     // {
    //     //     // Debug.LogWarning("something was tweaked on ExtendedBoxEditor's inspector.");
    //     // }
    //
    //     // if (GUILayout.Button("Snap lower bounds"))
    //     // {
    //     //     Debug.LogWarning("Snapping lower bounds.");
    //     //     var center = boxCollider.center;
    //     // }
    //     
    //     // if (serializedObject.ApplyModifiedProperties())
    //     // {
    //         // Debug.LogWarning("something was applied in ExtendedBoxEditor");
    //     // }
    //
    //     // if (prevCenterValue != centerProp.vector3Value)
    //     // {
    //     //     Debug.LogWarning("something changed to the value of center!");
    //     // }
    // }
}
