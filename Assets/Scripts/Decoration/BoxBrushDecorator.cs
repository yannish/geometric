using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.Serialization;


[Serializable]
public class BoxBrushDebug
{
    public bool drawFaceOrientation;
    public float drawCenterSize = 0.3f;
    public float drawSpaceSize = 0.5f;
    public float drawAAThickness = 3f;
    public Vector3 drawWireCubeSize = Vector3.one;
}

// [Flags]
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
    [Clamp(0, BoxBrushDecorator.MAX_INSTANCES_PER_FACE)]
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

    // public float calculatedPropSize;
    
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

[RequireComponent(typeof(BoxCollider))]
public class BoxBrushDecorator : MonoBehaviour
    , ISerializationCallbackReceiver
{
    public BoxBrushDebug debug;
    
    public GameObject prefab;
    
    public float calculatedPrefabSize;

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


    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        using (ColorPick.Swatches.boxBrushWire.ctx)
        {
            var prevHandlesMatrix = Handles.matrix;
            var prevHandlesDrawTest = Handles.zTest;
            Handles.zTest = CompareFunction.Less;
            Handles.matrix = transform.localToWorldMatrix;
            Handles.DrawWireCube(Vector3.zero, new Vector3(dimensions.width, dimensions.height, dimensions.depth));
            Handles.matrix = prevHandlesMatrix;
            Handles.zTest = prevHandlesDrawTest;
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

        var prevHandlesMatrix = Handles.matrix;
        var prevGizmosMatrix = Gizmos.matrix;
        
        Handles.matrix = transform.localToWorldMatrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        var instanceDrawSize = prefab != null ? calculatedPrefabSize : tempPrefabSize;
        instanceDrawSize = Mathf.Max(0.1f, instanceDrawSize);
        
        for (int i = 0; i < faceStates.Length; i++)
        {
            var face = faceStates[i];

            if (debug.drawFaceOrientation)
            {
                Gizmos.DrawWireSphere(face.center, debug.drawCenterSize);
                
                using(new ColorContext(Color.blue))
                    Handles.DrawAAPolyLine(debug.drawAAThickness, face.center, face.center + face.normal * debug.drawSpaceSize);

                using(new ColorContext(Color.yellow))
                    Handles.DrawAAPolyLine(debug.drawAAThickness, face.center, face.center + face.tangent * debug.drawSpaceSize);

                using(new ColorContext(Color.red))
                    Handles.DrawAAPolyLine(debug.drawAAThickness, face.center, face.center + face.bitangent * debug.drawSpaceSize);
            }
            
            if (face.isMuted)
                continue;
            
            //... draw our final positions:
            using (ColorPick.Swatches.boxBrushCenter.ctx)
            {
                foreach (var localPos in face.positions)
                {
                    Handles.DrawWireCube(localPos, Vector3.one * instanceDrawSize);
                }
            }
        }
        
        Handles.matrix = prevHandlesMatrix;
        Gizmos.matrix = prevGizmosMatrix;
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

    public void OnBeforeSerialize()
    {
        // Debug.LogWarning("on before serialized boxBrush!");
    }

    public void OnAfterDeserialize()
    {
        Debug.LogWarning("on after serialized boxBrush!");
    }
}
