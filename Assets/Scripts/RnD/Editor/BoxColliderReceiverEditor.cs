using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoxColliderReceiver))]
public class BoxColliderReceiverEditor : Editor
{
    BoxColliderReceiver boxColliderReceiver;
    BoxCollider boxCollider;
    BoxColliderExtendedEditor boxColliderExtendedEditor;
    private void OnEnable()
    {
        Debug.LogWarning("Enabled receiver");
        boxColliderReceiver = (BoxColliderReceiver)target;
        boxCollider = boxColliderReceiver.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogWarning("BoxCollider not found");
            return;
        }
        
        ActiveEditorTracker tracker = ActiveEditorTracker.sharedTracker;
        Editor[] editors = tracker.activeEditors;

        foreach (Editor editor in editors)
        {
            if (editor.target is BoxCollider)
            {
                Debug.LogWarning("found extended editor.");
                boxColliderExtendedEditor = editor as BoxColliderExtendedEditor;
                boxColliderExtendedEditor.OnBoxColliderChanged -= ReportBoxColliderEdit;
                boxColliderExtendedEditor.OnBoxColliderChanged += ReportBoxColliderEdit;
                break;
            }
        }
    }

    private void OnDisable()
    {
        Debug.LogWarning("Disabled receiver");
        if(boxColliderExtendedEditor != null)
            boxColliderExtendedEditor.OnBoxColliderChanged -= ReportBoxColliderEdit;
    }

    void ReportBoxColliderEdit()
    {
        Debug.LogWarning("Box collider edit detected.");
    }
}
