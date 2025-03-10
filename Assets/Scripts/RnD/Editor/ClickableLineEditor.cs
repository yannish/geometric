using System;
using Unity.Burst.Intrinsics;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ClickableLine))]
public class ClickableLineEditor : Editor
{
    public void OnSceneGUI()
    {
        var clickableLine = (ClickableLine)target;
        Vector3 p1 = clickableLine.transform.position;
        Vector3 p2 = clickableLine.transform.position + clickableLine.transform.forward * 5f;

        Vector2 p1Screen = HandleUtility.WorldToGUIPoint(p1);
        Vector2 p2Screen = HandleUtility.WorldToGUIPoint(p2);
        
        using (ColorPick.Swatches.salmon.ctx)
        {
            Handles.DrawAAPolyLine(3f, p1, p2);
        }
        
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Vector3 mousePos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;

            float dist = HandleUtility.DistancePointToLineSegment(
                e.mousePosition,
                p1Screen, p2Screen
            );

            if (dist < clickableLine.clickableThreshold)
            {
                Debug.LogWarning("CLICKED LINE!");
                e.Use();
            }
        }
    }
}
