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
    public float drawCornerArrowSize = 3f;

    public float drawDottedLineSpacing = 0.2f;
    
    public float elementSelectionSize = 1f;
    public float elementSelectedInflation = 1.2f;
    public float elementPickSize = 2f;
    
    public Vector3 drawWireCubeSize = Vector3.one;
}

[Serializable]
public class BoxDecoratorCornerSettings
{
    public float insetAmount = 0.25f;
}

[Serializable]
public class BoxDecoratorFaceSettings
{
    public float padding;
    public float spacing;
    public bool fill;
    public int instanceCount = 4;
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

    public bool overrideFill;
    [ShowIf("overrideFill")]
    public bool fill;

    public bool overrideInstanceCount;
    [ShowIf("overridePadding"), Clamp(0, BoxBrushDecorator.MAX_INSTANCES_PER_FACE)]
    public int numInstances;

    public bool overridePadding;
    [ShowIf("overridePadding")]
    public float padding;
    
    public bool overrideSpacing;
    [ShowIf("overrideSpacing")]
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
public class BoxBrushDecoratorCorner
{
    //... CONFIGURED:
    public bool isMuted;
    public bool overrideInsetAmount;
    [ShowIf("overrideInsetAmount")]
    public float insetAmount = 0.25f;
    
    //... CALCULATED:
    public Vector3 position;
    public Vector3 insetPosition;
    public GameObject instance;
    
    //... PRE-BAKED:
    public Vector3 normal; //... this is flattened-out, but maybe that's an optional thing?
    public BoxBrushCornerType direction;
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
    public string decoratorName;
    
    public BoxBrushDebug debug;
    
    public GameObject prefab;
    
    public float calculatedPrefabSize;

    public float tempPrefabSize = 1f;
    
    public BoxBrushDimensions dimensions;
    
    public BoxBrushDecorationType type;

    private BoxCollider _boxCollider;
    BoxCollider BoxCollider
    {
        get
        {
            if(_boxCollider == null)
                _boxCollider = GetComponent<BoxCollider>();
            return _boxCollider;
        }
    }
    
    #region PROPERTIES:
    public Vector3 dims => BoxCollider.size;
    public Vector3 halfDims => dims * 0.5f;
    // public Vector3 dims => new Vector3(dimensions.width, dimensions.height, dimensions.depth);
    #endregion
    
    
    #region FACES
    public BoxDecoratorFaceSettings faceSettings;
    public const int MAX_INSTANCES_PER_FACE = 100;
    public BoxBrushDecoratorFace[] faceStates = new BoxBrushDecoratorFace[4];
    #endregion
    
    #region CORNERS
    public BoxDecoratorCornerSettings cornerSettings;
    public BoxBrushDecoratorCorner[] cornerStates;// = new BoxBrushDecoratorCorner[8];
    #endregion
    
    #region EDGES
    #endregion

    private void Reset()
    {
        
    }

    [ContextMenu("Initialize Corners")]
    public void InitializeCorners()
    {
        cornerStates = new BoxBrushDecoratorCorner[8];
        var keys = BoxBrushDirections.cornerNormalLookup.Keys;
        int i = 0;
        foreach (var kvp in BoxBrushDirections.cornerNormalLookup)
        {
            cornerStates[i] = new BoxBrushDecoratorCorner();
            cornerStates[i].direction = kvp.Key;
            cornerStates[i].position = Vector3.Scale(kvp.Value, halfDims);
            cornerStates[i].insetAmount = cornerSettings.insetAmount;
            // var effectiveInset = cornerStates[i].overrideInsetAmount ?
            cornerStates[i].insetPosition = cornerStates[i].position + cornerStates[i].normal * cornerStates[i].insetAmount;
            cornerStates[i].normal = -kvp.Value.FlatInXZ().normalized;
            i++;
        }
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        using (ColorPick.Swatches.boxBrushWire.ctx)
        {
            var prevHandlesMatrix = Handles.matrix;
            var prevHandlesDrawTest = Handles.zTest;
            Handles.zTest = CompareFunction.Less;
            Handles.matrix = transform.localToWorldMatrix;
            Handles.DrawWireCube(Vector3.zero, new Vector3(dims.x, dims.y, dims.z));
            Handles.matrix = prevHandlesMatrix;
            Handles.zTest = prevHandlesDrawTest;
        }
        
        DrawCorners();
        DrawEdges();
        DrawFaces();
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
        if (type != BoxBrushDecorationType.CORNER)
            return;
        
        var prevHandlesMatrix = Handles.matrix;
        var prevGizmosMatrix = Gizmos.matrix;
        
        Handles.matrix = transform.localToWorldMatrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        var instanceDrawSize = prefab != null ? calculatedPrefabSize : tempPrefabSize;
        instanceDrawSize = Mathf.Max(0.1f, instanceDrawSize);
        
        for (int i = 0; i < cornerStates.Length; i++)
        {
            var corner = cornerStates[i];
            // var effectiveInset = corner.overrideInsetAmount ? corner.insetAmount : cornerSettings.insetAmount;

            if (debug.drawFaceOrientation)
            {
                Gizmos.DrawWireSphere(corner.position, debug.drawCenterSize);

                //... draw corner normal:
                using (new ColorContext(Color.blue))
                {
                    Handles.ArrowHandleCap(
                        -1,
                        corner.position + Vector3.up * 0.2f,
                        Quaternion.LookRotation(corner.normal),
                        debug.drawCornerArrowSize,
                        EventType.Repaint
                        );
                    
                    Handles.DrawAAPolyLine(
                        debug.drawAAThickness,
                        corner.position, corner.position + corner.normal * debug.drawSpaceSize
                        );
                }

                // using(new ColorContext(Color.yellow))
                //     Handles.DrawAAPolyLine(debug.drawAAThickness, corner.position, corner.position + Vector3.up * debug.drawSpaceSize);
                //
                // using(new ColorContext(Color.red))
                //     Handles.DrawAAPolyLine(debug.drawAAThickness, corner.position, corner.position + Vector3.right * debug.drawSpaceSize);
            }
            
            if (corner.isMuted)
                continue;
            
            //... draw our final positions:
            using (ColorPick.Swatches.boxBrushCenter.ctx)
            {
                Handles.DrawWireCube(corner.insetPosition, Vector3.one * instanceDrawSize);
                Handles.DrawDottedLine(corner.position, corner.insetPosition, debug.drawDottedLineSpacing);
            }
        }
        
        Handles.matrix = prevHandlesMatrix;
        Gizmos.matrix = prevGizmosMatrix;
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
        // Debug.LogWarning("on after serialized boxBrush!");
    }
}
