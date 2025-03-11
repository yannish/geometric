using System;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public static class BoxBrushDecoratorActions
{
    public static void ClearDecoratorFaceInstance(BoxBrushDecorator decorator, BoxBrushDecoratorFace face)
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
        Debug.LogWarning("Reinstantiating prefabs!");

        // var allChildGameObjects = decorator.gameObject
        //     .GetComponentsInChildren<Transform>(true)
        //     .Where(t => t != decorator.transform);
        //
        // foreach(var instance in allChildGameObjects) 
        //     GameObject.DestroyImmediate(instance.gameObject);
        
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
        
        float faceLength = decorator.dimensions.height;
        if (face.orientation == BoxBrushFaceDecoratorOrientation.ALONG_WIDTH)
        {
            switch (face.direction)
            {
                case BoxBrushDirection.FORWARD:
                case BoxBrushDirection.BACKWARD:
                    faceLength = decorator.dimensions.width;
                    break;
                
                case BoxBrushDirection.LEFT:
                case BoxBrushDirection.RIGHT:
                    faceLength = decorator.dimensions.depth;
                    break;
            }
        }

        float faceDistance = 0f;
        switch (face.direction)
        {
            case BoxBrushDirection.FORWARD:
            case BoxBrushDirection.BACKWARD:
                faceDistance = decorator.dimensions.depth * 0.5f;
                break;
            
            case BoxBrushDirection.LEFT:
            case BoxBrushDirection.RIGHT:
                faceDistance = decorator.dimensions.width * 0.5f;
                break;
            
            case BoxBrushDirection.UP:
            case BoxBrushDirection.DOWN:
                faceDistance = decorator.dimensions.height * 0.5f;
                break;
        }

        face.normal = -BoxBrushDirections.brushDirLookup[face.direction];
        face.tangent = BoxBrushDirections.tangentLookup[face.direction];
        face.bitangent = BoxBrushDirections.bitangentLookup[face.direction];
        face.center = -face.normal * faceDistance;

        face.effectiveSpan = faceLength - 2f * face.padding;

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
        
        var effectiveNumInstances = !face.fill 
            ? Mathf.Clamp(face.numInstances, 0, maxInstancesBySize) 
            : maxInstancesBySize;
        
        effectiveNumInstances = Mathf.Clamp(effectiveNumInstances, 0, BoxBrushDecorator.MAX_INSTANCES_PER_FACE);
        
        // if(effectiveNumInstances > 0)
        //     Debug.LogWarning($"effectiveNumInstances: {effectiveNumInstances}");
        
        var totalInnerPadding = face.effectiveSpan - effectiveNumInstances * clampedInstanceSize;
        var separationPadding = effectiveNumInstances <= 1 
            ? 0f
            : totalInnerPadding / (effectiveNumInstances - 1);
        
        //... TODO: if we're applying "spacing", it will act like a min on this separation padding, and 
        //... then effectiveNumInstances will need to be recalculated.
         
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

    public static void RecalculateCorner(this BoxBrushDecorator decorator, BoxBrushDecoratorCorner corner)
    {
        var cornerDir = BoxBrushDirections.cornerNormalLookup[corner.direction];
        corner.position = Vector3.Scale(cornerDir, decorator.haldDims);
        corner.insetPosition = corner.position + corner.normal * corner.insetAmount;
    }
}
