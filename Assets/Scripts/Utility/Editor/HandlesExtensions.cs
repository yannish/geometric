using UnityEditor;
using UnityEngine;

public static class HandlesExtensions
{
    public static void DrawDashedBox(Vector3 center, Vector3 size, Color color, float thickness)
    {
        Vector3 p1 = center + size * 0.5f;
        Vector3 p2 = center - size * 0.5f;
        // Handles.DrawD
        // Handles.DrawDottedLines();
    }
}
