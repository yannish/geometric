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
public class BoxDecoratorEdgeSettings
{
    public bool fill;
    [Clamp(0f, Mathf.Infinity)]
    public float padding;
    [Clamp(0f, Mathf.Infinity)]
    public float spacing;
    [Clamp(0, BoxBrushDecorator.MAX_INSTANCES_PER_EDGE)]
    public int instanceCount = 4;
}

[Serializable]
public class BoxDecoratorFaceSettings
{
    [Clamp(0f, Mathf.Infinity)]
    public float padding;
    [Clamp(0f, Mathf.Infinity)]
    public float spacing;
    public bool fill;
    [Clamp(0, BoxBrushDecorator.MAX_INSTANCES_PER_FACE)]
    public int instanceCount = 4;
}

public enum BoxBrushDecorationType
{
    FACE = 1 << 0,
    CORNER = 1 << 1,
    EDGE = 1 << 2,
}


public enum BoxBrushFaceDecoratorOrientation
{
    ALONG_WIDTH,
    ALONG_HEIGHT,
}

[Serializable]
public class BoxBrushDecoratorFace
{
    //... BAKED:
    [HideInInspector]
    public BoxBrushDirection direction;
    
    //... CONFIGURED:
    public bool isMuted;
    public BoxBrushFaceDecoratorOrientation orientation;

    public bool overrideFill;
    [ShowIf("overrideFill")]
    public bool fill;

    public bool overrideInstanceCount;
    [ShowIf("overridePadding"), Clamp(0, BoxBrushDecorator.MAX_INSTANCES_PER_FACE)]
    public int numInstances;

    public bool overridePadding;
    [ShowIf("overridePadding"), Clamp(0f, Mathf.Infinity)]
    public float padding;
    
    public bool overrideSpacing;
    [ShowIf("overrideSpacing"), Clamp(0f, Mathf.Infinity)]
    public float spacing;
    
    
    //... CALCULATED:
    [HideInInspector] public Vector3 center;
    [HideInInspector] public Vector3 normal;
    [HideInInspector] public Vector3 tangent;
    [HideInInspector] public Vector3 bitangent;
    [HideInInspector] public float span;
    [HideInInspector] public float effectiveSpan;
    [HideInInspector] public List<Vector3> positions = new List<Vector3>();
    [HideInInspector] public List<GameObject> instances =  new List<GameObject>();
}

[Serializable]
public class BoxBrushDecoratorCorner
{
    //... PRE-BAKED:
    // [HideInInspector]
    public BoxBrushCornerType direction;
    public Vector3 normal; //... this is flattened-out, but maybe that's an optional thing?
    
    //... CONFIGURED:
    public bool isMuted;
    public bool overrideInsetAmount;
    [ShowIf("overrideInsetAmount")]
    public float insetAmount = 0.25f;
    
    //... CALCULATED:
    public Vector3 position;
    public Vector3 insetPosition;
    public GameObject instance;
}

[Serializable]
public class BoxBrushDecoratorEdge
{
    [HideInInspector]
    public BoxBrushEdge type;
    
    //... CONFIGURED:
    public bool isMuted;

    public bool overrideFill;
    [ShowIf("overrideFill")]
    public bool fill;

    public bool overrideInstanceCount;
    [ShowIf("overridePadding"), Clamp(0, BoxBrushDecorator.MAX_INSTANCES_PER_FACE)]
    public int numInstances;

    public bool overridePadding;
    [ShowIf("overridePadding"), Clamp(0f, Mathf.Infinity)]
    public float padding;
    
    public bool overrideSpacing;
    [ShowIf("overrideSpacing"), Clamp(0f, Mathf.Infinity)]
    public float spacing;
    
    
    public Vector3 center;
    public Vector3 normal;
    public Vector3 tangent;
    public Vector3 bitangent;
    public float span;
    public float effectiveSpan;
    [HideInInspector] public List<Vector3> positions = new List<Vector3>();
    [HideInInspector] public List<GameObject> instances =  new List<GameObject>();
}

// [Serializable]
// public struct BoxBrushDimensions
// {
//     public float width;
//     public float height;
//     public float depth;
// }

[SelectionBase]
[RequireComponent(typeof(BoxCollider))]
public class BoxBrushDecorator : MonoBehaviour
    , ISerializationCallbackReceiver
{
    [Clamp(0f, Mathf.Infinity)]
    public float quickFloat;
    
    public string decoratorName;
    
    public BoxBrushDebug debug;
    
    public GameObject prefab;
    
    public float calculatedPrefabSize;

    public float tempPrefabSize = 1f;
    
    // public BoxBrushDimensions dimensions;
    
    public BoxBrushDecorationType type;

    private BoxCollider _boxCollider;
    public BoxCollider BoxCollider
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
    public BoxDecoratorEdgeSettings edgeSettings;
    public const int MAX_INSTANCES_PER_EDGE = 100;
    public BoxBrushDecoratorEdge[] edgeStates = new BoxBrushDecoratorEdge[12];
    #endregion

    private void Reset()
    {
        
    }

    [HideInInspector, SerializeField] public int selectedFace = -1;
    [HideInInspector, SerializeField] public int selectedCorner = -1;
    [HideInInspector, SerializeField] public int selectedEdge = -1;
    
    [ContextMenu("Initialize Corners")]
    public void InitializeCorners()
    {
        cornerStates = new BoxBrushDecoratorCorner[8];
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

    [ContextMenu("Initialize Faces")]
    public void InitializeFaces()
    {
        this.ClearFaces();
        faceStates = new BoxBrushDecoratorFace[6];
        int i = 0;
        foreach (var kvp in BoxBrushDirections.faceDirLookup)
        {
            faceStates[i] = new BoxBrushDecoratorFace();
            faceStates[i].direction = kvp.Key;
            i++;
        }
    }

    [ContextMenu("Initialize Edges")]
    public void InitializeEdges()
    {
        this.ClearEdges();
        edgeStates = new BoxBrushDecoratorEdge[12];
        int i = 0;
        foreach (var kvp in BoxBrushDirections.edgeCenterLookup)
        {
            edgeStates[i] = new BoxBrushDecoratorEdge();
            edgeStates[i].type = kvp.Key;
            edgeStates[i].center = Vector3.Scale(kvp.Value, halfDims);
            edgeStates[i].normal = -kvp.Value.normalized;
            edgeStates[i].tangent = BoxBrushDirections.edgeTangentLookup[kvp.Key];
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

            if(face.positions == null || face.positions.Count == 0)
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
                Handles.DrawWireDisc(corner.insetPosition, corner.normal, instanceDrawSize);
                // Handles.DrawWireCube(corner.insetPosition, Vector3.one * instanceDrawSize);
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
        
        var prevHandlesMatrix = Handles.matrix;
        var prevGizmosMatrix = Gizmos.matrix;
        
        Handles.matrix = transform.localToWorldMatrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        for (int i = 0; i < edgeStates.Length; i++)
        {
            var edge = edgeStates[i];
            
            if (debug.drawFaceOrientation)
            {
                Gizmos.DrawWireSphere(edge.center, debug.drawCenterSize);

                //... draw corner normal:
                using (new ColorContext(Color.blue))
                {
                    Handles.ArrowHandleCap(
                        -1,
                        edge.center,
                        Quaternion.LookRotation(edge.normal),
                        debug.drawCornerArrowSize,
                        EventType.Repaint
                    );
                    
                    // Handles.DrawAAPolyLine(
                    //     debug.drawAAThickness,
                    //     corner.position, corner.position + corner.normal * debug.drawSpaceSize
                    // );
                }
                
                using (new ColorContext(Color.red))
                {
                    Handles.ArrowHandleCap(
                        -1,
                        edge.center,
                        Quaternion.LookRotation(edge.tangent),
                        debug.drawCornerArrowSize,
                        EventType.Repaint
                    );
                    
                    // Handles.DrawAAPolyLine(
                    //     debug.drawAAThickness,
                    //     corner.position, corner.position + corner.normal * debug.drawSpaceSize
                    // );
                }

                // using(new ColorContext(Color.yellow))
                //     Handles.DrawAAPolyLine(debug.drawAAThickness, corner.position, corner.position + Vector3.up * debug.drawSpaceSize);
                //
                // using(new ColorContext(Color.red))
                //     Handles.DrawAAPolyLine(debug.drawAAThickness, corner.position, corner.position + Vector3.right * debug.drawSpaceSize);
            }
            
            if (edge.isMuted)
                continue;
            
            //... draw our final positions:
            using (ColorPick.Swatches.boxBrushCenter.ctx)
            {
                Handles.DrawWireDisc(edge.center, edge.normal, calculatedPrefabSize);
                // Handles.DrawWireCube(corner.insetPosition, Vector3.one * instanceDrawSize);
                // Handles.DrawDottedLine(corner.position, corner.insetPosition, debug.drawDottedLineSpacing);
            }
        }
        
        Handles.matrix = prevHandlesMatrix;
        Gizmos.matrix = prevGizmosMatrix;
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
