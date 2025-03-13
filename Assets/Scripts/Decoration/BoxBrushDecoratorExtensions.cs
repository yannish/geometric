using System;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public static class BoxBrushDecoratorExtensions
{
    public static void ClearEdges(this BoxBrushDecorator decorator)
    {
        foreach (var edge in decorator.edgeStates)
        {
            if(edge == null)
                continue;
            ClearEdge(decorator, edge);
        }
    }

    public static void ClearEdge(BoxBrushDecorator decorator, BoxBrushDecoratorEdge edge)
    {
        if (edge.instances != null)
        {
            foreach(var instance in edge.instances)
                GameObject.DestroyImmediate(instance);
            
            edge.instances.Clear();
        }
    }

    public static bool RecalculateEdge(BoxBrushDecorator decorator, BoxBrushDecoratorEdge edge)
    {
        bool instanceCountChanged = false;
        int prevInstanceCount = edge.positions.Count;
        float edgeLength = -1f;
        switch (edge.type)
        {
            case BoxBrushEdge.FRONT_TOP:
            case BoxBrushEdge.FRONT_BOTTOM:
            case BoxBrushEdge.BACK_TOP:
            case BoxBrushEdge.BACK_BOTTOM:
                edgeLength = decorator.dims.x;
                break;
            
            case BoxBrushEdge.FRONT_LEFT:
            case BoxBrushEdge.FRONT_RIGHT:
            case BoxBrushEdge.BACK_LEFT:
            case BoxBrushEdge.BACK_RIGHT:
                edgeLength = decorator.dims.y;
                break;
            
            case BoxBrushEdge.MID_TOP_RIGHT:
            case BoxBrushEdge.MID_TOP_LEFT:
            case BoxBrushEdge.MID_BOTTOM_RIGHT:
            case BoxBrushEdge.MID_BOTTOM_LEFT:
                edgeLength = decorator.dims.z;
                break;
        }
        
        edge.center = decorator.center + Vector3.Scale(decorator.halfDims, BoxBrushDirections.edgeCenterLookup[edge.type]);
        edge.normal = -BoxBrushDirections.edgeCenterLookup[edge.type].normalized;
        edge.bitangent = BoxBrushDirections.edgeTangentLookup[edge.type];
        
        float effectivePadding = edge.overridePadding ? edge.padding : decorator.edgeSettings.padding;
        edge.effectiveSpan = edgeLength - 2f * effectivePadding;
        
        //.. TODO: this should happen elsewhere?
        decorator.calculatedPrefabSize = -1f;
        if (decorator.prefab != null)
        {
            bool useCollisionCheckInstead = false;
            if (useCollisionCheckInstead)
            {
                var prefabInstance = PrefabUtility.InstantiatePrefab(decorator.prefab) as GameObject;
                var calculatedBounds = new Bounds(prefabInstance.transform.position, Vector3.zero);
                var colliders = prefabInstance.GetComponentsInChildren<Collider>();
                foreach (var col in colliders)
                {
                    calculatedBounds.Encapsulate(col.bounds);
                }
                GameObject.DestroyImmediate(prefabInstance);
                
                decorator.calculatedPrefabSize = calculatedBounds.size.x;
            }
            else
            {
                var calculatedBounds = new Bounds(decorator.prefab.transform.position, Vector3.zero);
                var meshRenderers = decorator.prefab.GetComponentsInChildren<MeshRenderer>();
                foreach (var meshRenderer in meshRenderers)
                {
                    calculatedBounds.Encapsulate(meshRenderer.bounds);
                }
        
                decorator.calculatedPrefabSize = calculatedBounds.size.x;
            }
        }
        
        float clampedInstanceSize = Mathf.Max(0.1f, decorator.calculatedPrefabSize);
        
        int maxInstancesBySize = Mathf.FloorToInt(edge.effectiveSpan / clampedInstanceSize);

        float effectiveSpacing = edge.overrideSpacing ? edge.spacing : decorator.edgeSettings.spacing;
        
        int filledInstanceCount =
            Mathf.FloorToInt((edge.effectiveSpan - effectiveSpacing) / (clampedInstanceSize + effectiveSpacing));

        var effectiveFill = edge.overrideFill ? edge.fill : decorator.edgeSettings.fill;

        var effectiveNumInstances = effectiveFill 
            ? filledInstanceCount
            : Mathf.Clamp(edge.numInstances, 0, maxInstancesBySize) ;
        
        effectiveNumInstances = Mathf.Clamp(effectiveNumInstances, 0, BoxBrushDecorator.MAX_INSTANCES_PER_EDGE);

        var totalInnerPadding = edge.effectiveSpan - effectiveNumInstances * clampedInstanceSize;
        var separationPadding = effectiveNumInstances <= 1 
            ? 0f
            : totalInnerPadding / (effectiveNumInstances - 1);
        
        edge.positions.Clear();
        
        if (effectiveNumInstances < 1)
            return prevInstanceCount != edge.positions.Count;
        
        if (effectiveNumInstances == 1)
        {
            edge.positions.Add(edge.center);
        }
        else
        {
            Vector3 startSpan = edge.center - (edge.effectiveSpan - clampedInstanceSize) * 0.5f * edge.bitangent;
            edge.positions.Add(startSpan);
            for (int i = 1; i < effectiveNumInstances; i++)
            {
                var spanStep = i * (separationPadding + clampedInstanceSize);
                edge.positions.Add(startSpan + spanStep * edge.bitangent);
            }
        }
        
        instanceCountChanged = prevInstanceCount != edge.positions.Count;
        
        return instanceCountChanged;
    }
    
    public static void ClearFaces(this BoxBrushDecorator decorator)
    {
        foreach (var face in decorator.faceStates)
        {
            if(face == null)
                continue;
            ClearFace(decorator, face);
        }
    }
    
    public static void ClearFace(BoxBrushDecorator decorator, BoxBrushDecoratorFace face)
    {
        if (face.instances != null)
        {
            foreach(var instance in face.instances) 
                GameObject.DestroyImmediate(instance);

            face.instances.Clear();
        }
    }
    
    public static void RealignDecoratorFaceInstances(BoxBrushDecorator decorator, BoxBrushDecoratorFace face)
    {
        if (face.positions.Count != face.instances.Count)
            return;
        
        // Debug.LogWarning($"realigning decorator instances, posCount {face.positions.Count}, {face.instances.Count} ");
        
        for (int i = 0; i < face.positions.Count; i++)
        {
            var instance = face.instances[i];
            if (instance == null)
            {
                Debug.LogWarning("instance of decorator prefab was null!", decorator.gameObject);
                return;
            }

            instance.transform.SetLocalPositionAndRotation(
                face.positions[i], 
                Quaternion.LookRotation(face.normal, face.tangent)
                );
        }
    }
    
    public static void RealignEdgeInstances(BoxBrushDecorator decorator, BoxBrushDecoratorEdge edge)
    {
        if (edge.positions.Count != edge.instances.Count)
            return;
        
        // Debug.LogWarning($"realigning decorator instances, posCount {face.positions.Count}, {face.instances.Count} ");
        
        for (int i = 0; i < edge.positions.Count; i++)
        {
            var instance = edge.instances[i];
            if (instance == null)
            {
                Debug.LogWarning("instance of decorator prefab was null!", decorator.gameObject);
                return;
            }

            var effectiveRot = decorator.edgeSettings.alignmentStyle == BoxDecoratorEdgeSettings.EdgeAlignmentStyle.WITH_NORMAL
                ? Quaternion.LookRotation(edge.normal, Vector3.Cross(edge.normal, edge.tangent))
                : Quaternion.LookRotation(edge.normal.FlatInXZ());
            
            instance.transform.SetLocalPositionAndRotation(edge.positions[i], effectiveRot);
        }
    }

    public static void RegenerateFaceInstances(BoxBrushDecorator decorator, BoxBrushDecoratorFace face)
    {
        // Debug.LogWarning("Reinstantiating prefabs!");
        
        foreach(var instance in face.instances) 
            GameObject.DestroyImmediate(instance);

        face.instances.Clear();
        
        if (decorator.prefab == null)
            return;
        
        for (int i = 0; i < face.positions.Count; i++)
        {
            var newPrefabInstance = PrefabUtility.InstantiatePrefab(decorator.prefab, decorator.transform) as GameObject;
            face.instances.Add(newPrefabInstance);
        }
    }
    
    public static void RegenerateEdgeInstances(BoxBrushDecorator decorator, BoxBrushDecoratorEdge edge)
    {
        // Debug.LogWarning("Reinstantiating prefabs!");
        
        foreach(var instance in edge.instances) 
            GameObject.DestroyImmediate(instance);

        edge.instances.Clear();
        
        if (decorator.prefab == null)
            return;
        
        for (int i = 0; i < edge.positions.Count; i++)
        {
            var newPrefabInstance = PrefabUtility.InstantiatePrefab(decorator.prefab, decorator.transform) as GameObject;
            edge.instances.Add(newPrefabInstance);
        }
    }
    
    public static bool RecalculateFace(BoxBrushDecorator decorator, BoxBrushDecoratorFace face)
    {
        bool instanceCountChanged = false;
        int prevInstanceCount = face.positions.Count;
        
        float faceLength = decorator.dims.y;
        if (face.orientation == BoxBrushFaceDecoratorOrientation.ALONG_WIDTH)
        {
            switch (face.direction)
            {
                case BoxBrushDirection.FORWARD:
                case BoxBrushDirection.BACKWARD:
                    faceLength = decorator.dims.x;
                    break;
                
                case BoxBrushDirection.LEFT:
                case BoxBrushDirection.RIGHT:
                    faceLength = decorator.dims.z;
                    break;
            }
        }

        float faceDistance = 0f;
        switch (face.direction)
        {
            case BoxBrushDirection.FORWARD:
            case BoxBrushDirection.BACKWARD:
                faceDistance = decorator.dims.z * 0.5f;
                break;
            
            case BoxBrushDirection.LEFT:
            case BoxBrushDirection.RIGHT:
                faceDistance = decorator.dims.x * 0.5f;
                break;
            
            case BoxBrushDirection.UP:
            case BoxBrushDirection.DOWN:
                faceDistance = decorator.dims.y * 0.5f;
                break;
        }

        face.normal = -BoxBrushDirections.faceDirLookup[face.direction];
        face.tangent = BoxBrushDirections.tangentLookup[face.direction];
        face.bitangent = BoxBrushDirections.bitangentLookup[face.direction];
        face.center = decorator.center - face.normal * faceDistance;

        float effectivePadding = face.overridePadding ? face.padding : decorator.faceSettings.padding;
        face.effectiveSpan = faceLength - 2f * effectivePadding;

        //.. TODO: this should happen elsewhere?
        decorator.calculatedPrefabSize = -1f;
        if (decorator.prefab != null)
        {
            bool useCollisionCheckInstead = false;
            if (useCollisionCheckInstead)
            {
                var prefabInstance = PrefabUtility.InstantiatePrefab(decorator.prefab) as GameObject;
                var calculatedBounds = new Bounds(prefabInstance.transform.position, Vector3.zero);
                var colliders = prefabInstance.GetComponentsInChildren<Collider>();
                foreach (var col in colliders)
                {
                    calculatedBounds.Encapsulate(col.bounds);
                }
                GameObject.DestroyImmediate(prefabInstance);
                
                decorator.calculatedPrefabSize = calculatedBounds.size.x;
            }
            else
            {
                var calculatedBounds = new Bounds(decorator.prefab.transform.position, Vector3.zero);
                var meshRenderers = decorator.prefab.GetComponentsInChildren<MeshRenderer>();
                foreach (var meshRenderer in meshRenderers)
                {
                    calculatedBounds.Encapsulate(meshRenderer.bounds);
                }
        
                decorator.calculatedPrefabSize = calculatedBounds.size.x;
            }
        }

        float clampedInstanceSize = Mathf.Max(0.1f, decorator.calculatedPrefabSize);
        
        int maxInstancesBySize = Mathf.FloorToInt(face.effectiveSpan / clampedInstanceSize);
        
        // Debug.LogWarning($"maxInstancesBySize: {maxInstancesBySize}");

        float effectiveSpacing = face.overrideSpacing ? face.spacing : decorator.faceSettings.spacing;
        
        int filledInstanceCount =
            Mathf.FloorToInt((face.effectiveSpan - effectiveSpacing) / (clampedInstanceSize + effectiveSpacing));
        
        var effectiveFill = face.overrideFill ? face.fill : decorator.faceSettings.fill;
        
        var effectiveNumInstances = effectiveFill 
            ? filledInstanceCount
            : Mathf.Clamp(face.numInstances, 0, maxInstancesBySize) ;
        
        effectiveNumInstances = Mathf.Clamp(effectiveNumInstances, 0, BoxBrushDecorator.MAX_INSTANCES_PER_FACE);

        
        var totalInnerPadding = face.effectiveSpan - effectiveNumInstances * clampedInstanceSize;
        var separationPadding = effectiveNumInstances <= 1 
            ? 0f
            : totalInnerPadding / (effectiveNumInstances - 1);
        
        face.positions.Clear();

        if (effectiveNumInstances < 1)
            return prevInstanceCount != face.positions.Count;

        var effectiveSpanDirection = face.orientation == BoxBrushFaceDecoratorOrientation.ALONG_WIDTH
            ? face.bitangent
            : face.tangent;
        
        if (effectiveNumInstances == 1)
        {
            face.positions.Add(face.center);
        }
        else
        {
            Vector3 startSpan = face.center - (face.effectiveSpan - clampedInstanceSize) * 0.5f * effectiveSpanDirection;
            face.positions.Add(startSpan);
            for (int i = 1; i < effectiveNumInstances; i++)
            {
                var spanStep = i * (separationPadding + clampedInstanceSize);
                face.positions.Add(startSpan + spanStep * effectiveSpanDirection);
            }
        }

        instanceCountChanged = prevInstanceCount != face.positions.Count;
        
        // Debug.LogWarning($"pos count: {face.positions.Count}");
        
        return instanceCountChanged;
    }

    
    
    public static void ClearCorners(this BoxBrushDecorator decorator)
    {
        foreach (var corner in decorator.cornerStates)
        {
            GameObject.DestroyImmediate(corner.instance);
            corner.instance = null;
        }
    }
    
    public static void RecalculateCorners(this BoxBrushDecorator decorator)
    {
        foreach(var corner in decorator.cornerStates)
            decorator.RecalculateCorner(corner);
    }

    public static void UpdateCorners(this BoxBrushDecorator decorator)
    {
        foreach (var corner in decorator.cornerStates)
        {
            UpdateCorner(decorator, corner);
        }
    }

    private static void UpdateCorner(BoxBrushDecorator decorator, BoxBrushDecoratorCorner corner)
    {
        if (corner.isMuted)
        {
            GameObject.DestroyImmediate(corner.instance);
            corner.instance = null;
        }
        else
        {
            if (decorator.prefab != null)
            {
                if (corner.instance == null)
                    corner.instance = PrefabUtility.InstantiatePrefab(decorator.prefab, decorator.transform) as GameObject;
                corner.instance.transform.SetLocalPositionAndRotation(
                    corner.insetPosition,
                    Quaternion.LookRotation(corner.normal)
                    );
            }
        }
    }

    private static void RecalculateCorner(this BoxBrushDecorator decorator, BoxBrushDecoratorCorner corner)
    {
        var cornerDir = BoxBrushDirections.cornerNormalLookup[corner.direction];
        corner.position = decorator.center + Vector3.Scale(cornerDir, decorator.halfDims);
        var effectiveInset = corner.overrideInsetAmount
            ? corner.insetAmount
            : decorator.cornerSettings.insetAmount;
        corner.insetPosition = corner.position + corner.normal * effectiveInset;
    }
}
