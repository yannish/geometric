using System;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public static class BoxBrushDecoratorExtensions
{
    public static void ClearFaces(this BoxBrushDecorator decorator)
    {
        foreach (var face in decorator.faceStates)
        {
            ClearDecoratorFace(decorator, face);
        }
    }
    
    public static void ClearDecoratorFace(BoxBrushDecorator decorator, BoxBrushDecoratorFace face)
    {
        foreach(var instance in face.instances) 
            GameObject.DestroyImmediate(instance);

        face.instances.Clear();
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

    public static void RegeneratePrefabInstances(BoxBrushDecorator decorator, BoxBrushDecoratorFace face)
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
    
    public static bool RecalculateDecoratorFace(BoxBrushDecorator decorator, BoxBrushDecoratorFace face)
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
        face.center = -face.normal * faceDistance;

        float effectivePadding = face.overridePadding ? face.padding : decorator.faceSettings.padding;
        face.effectiveSpan = faceLength - 2f * effectivePadding;

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
        
        if (effectiveNumInstances == 1)
        {
            face.positions.Add(face.center);
        }
        else
        {
            Vector3 startSpan = face.center - (face.effectiveSpan - clampedInstanceSize) * 0.5f * face.bitangent;
            face.positions.Add(startSpan);
            for (int i = 1; i < effectiveNumInstances; i++)
            {
                var spanStep = i * (separationPadding + clampedInstanceSize);
                face.positions.Add(startSpan + spanStep * face.bitangent);
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
            if (corner.instance == null)
                corner.instance = PrefabUtility.InstantiatePrefab(decorator.prefab, decorator.transform) as GameObject;
            corner.instance.transform.SetLocalPositionAndRotation(
                corner.insetPosition,
                Quaternion.LookRotation(corner.normal)
                );
        }
    }

    private static void RecalculateCorner(this BoxBrushDecorator decorator, BoxBrushDecoratorCorner corner)
    {
        var cornerDir = BoxBrushDirections.cornerNormalLookup[corner.direction];
        corner.position = Vector3.Scale(cornerDir, decorator.halfDims);
        var effectiveInset = corner.overrideInsetAmount
            ? corner.insetAmount
            : decorator.cornerSettings.insetAmount;
        corner.insetPosition = corner.position + corner.normal * effectiveInset;
    }
}
