using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Serialization;


[Serializable]
public class BoxBrushDebug
{
    public float drawCenterSize = 0.3f;
    public float drawSpaceSize = 0.5f;
    public float drawAAThickness = 3f;
    public Vector3 drawWireCubeSize = Vector3.one;
}

[Flags]
public enum BoxBrushDecorationType
{
    FACE = 1 << 0,
    CORNER = 1 << 1,
    EDGE = 1 << 2,
}

public enum BoxBrushCornerDecorationFlags
{
    RIGHT_FRONT_TOP = 1 << 0,
    LEFT_FRONT_TOP = 1 << 1,
    
    RIGHT_FRONT_DOWN = 1 << 2,
    LEFT_FRONT_DOWN = 1 << 3,
    
    RIGHT_BACK_TOP = 1 << 4,
    LEFT_BACK_TOP = 1 << 5,
    
    RIGHT_BACK_DOWN = 1 << 6,
    LEFT_BACK_DOWN = 1 << 7,
}

public enum BoxBrushFaceDecoratorOrientation
{
    ALONG_WIDTH,
    ALONG_HEIGHT,
}

[Serializable]
public class BoxBrushDecoratorFace
{
    //... CONFIGURED:
    public bool isMuted;
    public BoxBrushDirection direction;
    public BoxBrushFaceDecoratorOrientation orientation;
    public bool fill;
    // [Range(0, BoxBrushDecorator.MAX_INSTANCES_PER_FACE)]
    public int numInstances;
    public float padding;
    public float spacing;
    public float instanceSize; // should be calc'd eventually
    
    //... CALCULATED:
    public Vector3 center;
    public Vector3 normal;
    public Vector3 tangent;
    public Vector3 bitangent;

    public float span;
    public float effectiveSpan;
    
    public List<Vector3> positions;
    public List<GameObject> instances;
}


[Serializable]
public struct BoxBrushDimensions
{
    public float width;
    public float height;
    public float depth;
}

public class BoxBrushDecorator : MonoBehaviour
{
    public BoxBrushDebug debug;
    
    public GameObject prefab;

    public float tempPrefabSize = 1f;
    
    public BoxBrushDimensions dimensions;
    
    public BoxBrushDecorationType type;

    
    #region FACES

    public const int MAX_INSTANCES_PER_FACE = 100;
    
    public BoxBrushDirection face;
    
    public BoxBrushDecoratorFace[] faceStates = new BoxBrushDecoratorFace[4];
    
    #endregion
    
    #region CORNERS
    #endregion
    
    #region EDGES
    #endregion

    private void OnValidate()
    {
        if (type.HasFlag(BoxBrushDecorationType.FACE))
        {
            // Debug.LogWarning("... recalculating brush faces.");
        
            for (int i = 0; i < 4; i++)
            {
                if (faceStates[i].isMuted)
                    continue;
                BoxBrushDecoratorActions.RecalculateDecoratorFace(this, faceStates[i]);
            }
        }
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        using (ColorPick.Swatches.boxBrushWire.ctx)
        {
            Handles.DrawWireCube(transform.position, new Vector3(dimensions.width, dimensions.height, dimensions.depth));
        }
        
        DrawFaces();
        DrawEdges();
        DrawCorners();
#endif
    }

    
    void DrawFaces()
    {
        if (!type.HasFlag(BoxBrushDecorationType.FACE))
            return;

        using (ColorPick.Swatches.boxBrushCenter.ctx)
        {
            for (int i = 0; i < faceStates.Length; i++)
            {
                var face = faceStates[i];
                if (face.isMuted)
                    continue;
                
                Gizmos.DrawWireSphere(face.center, debug.drawCenterSize);
                
                using(new ColorContext(Color.blue))
                    Handles.DrawAAPolyLine(debug.drawAAThickness, face.center, face.center + face.normal * debug.drawSpaceSize);

                using(new ColorContext(Color.yellow))
                    Handles.DrawAAPolyLine(debug.drawAAThickness, face.center, face.center + face.tangent * debug.drawSpaceSize);
    
                using(new ColorContext(Color.red))
                    Handles.DrawAAPolyLine(debug.drawAAThickness, face.center, face.center + face.bitangent * debug.drawSpaceSize);
                
                foreach(var pos in face.positions)
                    Handles.DrawWireCube(pos, Vector3.one * face.instanceSize);
            }
        }
    }
    
    void DrawCorners()
    {
        if (!type.HasFlag(BoxBrushDecorationType.CORNER))
            return;
    }
    
    void DrawEdges()
    {
        if (!type.HasFlag(BoxBrushDecorationType.EDGE))
            return;
    }
}
