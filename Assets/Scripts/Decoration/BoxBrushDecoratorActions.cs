using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public static class BoxBrushDecoratorActions
{
    public static void RealignDecoratorFaceInstances(BoxBrushDecorator decorator, BoxBrushDecoratorFace face)
    {
        if (decorator.prefab == null)
            return;
        
        for (int i = 0; i < face.positions.Count; i++)
        {
            var instance = face.instances[i];
            instance.transform.SetLocalPositionAndRotation(face.positions[i], Quaternion.identity);
        }
    }

    public static void RegeneratePrefabInstances(BoxBrushDecorator decorator, BoxBrushDecoratorFace face)
    {
        Debug.LogWarning("Reinstantiating prefabs!");

        foreach(var instance in face.instances) 
            GameObject.DestroyImmediate(instance);

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
        
        float clampedInstanceSize = Mathf.Max(0.1f, face.instanceSize);

        int maxInstancesBySize = Mathf.FloorToInt(face.effectiveSpan / clampedInstanceSize);
        
        Debug.LogWarning($"maxInstancesBySize: {maxInstancesBySize}");
        
        var effectiveNumInstances = !face.fill 
            ? Mathf.Clamp(face.numInstances, 0, maxInstancesBySize) 
            : maxInstancesBySize;
        
        effectiveNumInstances = Mathf.Clamp(effectiveNumInstances, 0, BoxBrushDecorator.MAX_INSTANCES_PER_FACE);
        
        if(effectiveNumInstances > 0)
            Debug.LogWarning($"effectiveNumInstances: {effectiveNumInstances}");
        
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
                var spanStep = i * (separationPadding + face.instanceSize);
                face.positions.Add(startSpan + spanStep * face.bitangent);
            }
        }

        instanceCountChanged = prevInstanceCount != face.positions.Count;
        
        Debug.LogWarning($"pos count: {face.positions.Count}");
        
        return instanceCountChanged;
    }
}
