using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;


// [InitializeOnLoad]
public static class OverlayRunner
{
    // static OverlayManager()
    // {
    //     Selection.selectionChanged += UpdateOverlayState;
    //     UpdateOverlayState(); // Initial check
    // }
    //
    // private static void UpdateOverlayState()
    // {
    //     bool shouldEnableOverlay = Selection.activeGameObject != null && Selection.activeGameObject.CompareTag("Player");
    //
    //     foreach (var overlay in EditorUtility.ove.)
    //     {
    //         if (overlay is MyCustomOverlay myOverlay)
    //         {
    //             myOverlay.enabled = shouldEnableOverlay; // Enable only if the selected object has the "Player" tag
    //             SceneView.lastActiveSceneView.Repaint(); // Refresh the Scene View
    //         }
    //     }
    // }
}